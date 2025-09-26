using UnityEngine;
using UnityEngine.InputSystem;

public class CameraFollow : MonoBehaviour
{
    [Header("Objetivo")]
    public Transform jugador;

    [Header("Zona Muerta (Estilo Hollow Knight)")]
    [Tooltip("Zona muerta horizontal - la cámara no se moverá hasta que el jugador salga de esta área")]
    public float anchoZonaMuerta = 4f;
    [Tooltip("Zona muerta vertical - la cámara no se moverá hasta que el jugador salga de esta área")]
    public float altoZonaMuerta = 2f;
    [Tooltip("Mostrar zona muerta en la vista de escena para depuración")]
    public bool mostrarGizmoZonaMuerta = true;

    [Header("Configuración de Seguimiento de Cámara")]
    [Tooltip("Qué tan rápido sigue la cámara cuando el jugador sale de la zona muerta")]
    public float velocidadSeguimiento = 2f;
    [Tooltip("Factor de suavizado para el movimiento de cámara (menor = más suave)")]
    public float amortiguacionSuave = 0.3f;
    private Vector3 velocidad = Vector3.zero;

    [Header("Sistema de Mirar Adelante")]
    [Tooltip("Qué tan lejos mirar adelante cuando el jugador se está moviendo")]
    public float distanciaLookAhead = 3f;
    [Tooltip("Qué tan rápido aplicar el look ahead")]
    public float suavidadLookAhead = 2f;
    [Tooltip("Velocidad mínima del jugador para activar el look ahead")]
    public float umbraLookAhead = 0.1f;
    private float lookAheadActual = 0f;

    [Header("Controles de Vista Vertical")]
    [Tooltip("Qué tan arriba se mueve la cámara al mirar hacia arriba")]
    public float desplazamientoMirarArriba = 3f;
    [Tooltip("Qué tan abajo se mueve la cámara al mirar hacia abajo")]
    public float desplazamientoMirarAbajo = -3f;
    [Tooltip("Qué tan rápido interpola la vista vertical")]
    public float velocidadVistaVertical = 3f;
    private float vistaVerticalActual = 0f;

    [Header("🕒 Sistema de Cooldown Vista Vertical")]
    [Tooltip("Tiempo de cooldown entre activaciones de vista vertical (en segundos)")]
    public float cooldownVistaVertical = 0.5f;
    [Tooltip("Tiempo mínimo que debe mantener presionado para activar vista vertical")]
    public float tiempoMantenerPresionado = 0.1f;
    [Tooltip("Mostrar logs de debug para el cooldown")]
    public bool mostrarDebugCooldown = true;

    // Variables del sistema de cooldown - SIMPLIFICADO
    private float timerCooldownVertical = 0f;
    private float timerMantenerPresionado = 0f;
    private bool puedeUsarVistaVertical = true;
    private bool inputManteniendose = false;
    private float direccionActual = 0f;

    [Header("Límites de Cámara")]
    public bool usarLimitesCamara = false;
    [Tooltip("La cámara no irá más allá de estos límites")]
    public Vector2 limitesMinimos;
    public Vector2 limitesMaximos;

    [Header("Características Hollow Knight")]
    [Tooltip("Sesgo ligeramente hacia arriba para enfocar en lo que está adelante")]
    public float sesgoVertical = 0.5f;
    [Tooltip("Qué tan rápido la cámara se ajusta a nuevos puntos de enfoque")]
    public float velocidadEnfoqueRapido = 1f;

    [Header("Sistema de Input")]
    public InputActionAsset accionesInput;

    // Acciones de input
    private InputAction accionMirar;
    private InputAction accionCamara;

    // Valores de input
    private Vector2 inputMirar;
    private float inputCamara;

    // Estado de la cámara
    private Vector3 posicionObjetivo;
    private Vector3 centroZonaMuerta;
    private Rigidbody rbJugador;

    public bool validar_inputs_camara = true;

    void Start()
    {
        ConfigurarAccionesInput();

        // Obtener rigidbody del jugador para cálculos de velocidad
        if (jugador != null)
        {
            rbJugador = jugador.GetComponent<Rigidbody>();
            centroZonaMuerta = jugador.position;
        }
    }

    void ConfigurarAccionesInput()
    {
        if (accionesInput == null)
        {
            accionesInput = Resources.Load<InputActionAsset>("InputSystem_Actions");
        }

        if (accionesInput != null)
        {
            var mapaAccionesJugador = accionesInput.FindActionMap("Player");
            if (mapaAccionesJugador != null)
            {
                accionMirar = mapaAccionesJugador.FindAction("Look");
            }
        }

        // ✅ CONTROLES CORREGIDOS - W=ARRIBA, S=ABAJO
        accionCamara = new InputAction("CamaraVertical", InputActionType.Value, expectedControlType: "Axis");
        accionCamara.AddCompositeBinding("1DAxis")
            .With("Positive", "<Keyboard>/w")    // W mueve cámara ARRIBA
            .With("Negative", "<Keyboard>/s");   // S mueve cámara ABAJO

        // Respaldo para acción de mirar
        if (accionMirar == null)
        {
            accionMirar = new InputAction("Mirar", InputActionType.Value, expectedControlType: "Vector2");
            accionMirar.AddBinding("<Gamepad>/rightStick");
        }

        ConfigurarCallbacksInput();
        HabilitarAccionesInput();
    }

    void ConfigurarCallbacksInput()
    {
        accionMirar.performed += AlMirar;
        accionMirar.canceled += AlMirar;
        accionCamara.performed += AlCamaraVertical;
        accionCamara.canceled += AlCamaraVertical;
    }

    void HabilitarAccionesInput()
    {
        accionMirar?.Enable();
        accionCamara?.Enable();
    }

    void DeshabilitarAccionesInput()
    {
        accionMirar?.Disable();
        accionCamara?.Disable();
    }

    void OnDestroy()
    {
        DeshabilitarAccionesInput();
    }

    void AlMirar(InputAction.CallbackContext context)
    {
        inputMirar = context.ReadValue<Vector2>();
    }

    void AlCamaraVertical(InputAction.CallbackContext context)
    {
        inputCamara = context.ReadValue<float>();

        if (mostrarDebugCooldown)
            Debug.Log($"🎮 Input Cámara: {inputCamara}");
    }

    void Update()
    {
        // 🕒 ACTUALIZAR COOLDOWN DE VISTA VERTICAL - SIMPLIFICADO
        ActualizarCooldownVistaVertical();
    }

    void ActualizarCooldownVistaVertical()
    {
        // Actualizar timer de cooldown
        if (timerCooldownVertical > 0f)
        {
            timerCooldownVertical -= Time.deltaTime;
            if (timerCooldownVertical <= 0f)
            {
                puedeUsarVistaVertical = true;
                if (mostrarDebugCooldown)
                    Debug.Log("✅ Cooldown vista vertical terminado - Disponible nuevamente");
            }
        }

        // Verificar si hay input vertical activo
        bool hayInputVertical = false;
        float direccionInput = 0f;

        if (validar_inputs_camara)
        {
            // Input de teclado tiene prioridad
            if (Mathf.Abs(inputCamara) > 0.1f)
            {
                hayInputVertical = true;
                direccionInput = Mathf.Sign(inputCamara);

                if (mostrarDebugCooldown)
                    Debug.Log($"🎯 Input teclado detectado: {direccionInput} (W=+1, S=-1)");
            }
            // Input del stick derecho del controlador
            else if (Mathf.Abs(inputMirar.y) > 0.1f)
            {
                hayInputVertical = true;
                direccionInput = Mathf.Sign(inputMirar.y);

                if (mostrarDebugCooldown)
                    Debug.Log($"🎮 Input stick detectado: {direccionInput}");
            }
        }

        // 🚀 LÓGICA SIMPLIFICADA DEL COOLDOWN
        if (hayInputVertical)
        {
            // Si no estaba manteniendo input antes, empezar contador
            if (!inputManteniendose)
            {
                timerMantenerPresionado = 0f;
                direccionActual = direccionInput;
                inputManteniendose = true;

                if (mostrarDebugCooldown)
                    Debug.Log($"⏱️ Iniciando timer vista vertical - Dirección: {(direccionInput > 0 ? "ARRIBA" : "ABAJO")}");
            }

            // Si cambió la dirección, resetear
            if (direccionInput != direccionActual)
            {
                timerMantenerPresionado = 0f;
                direccionActual = direccionInput;

                if (mostrarDebugCooldown)
                    Debug.Log($"🔄 Cambio de dirección - Nueva: {(direccionInput > 0 ? "ARRIBA" : "ABAJO")}");
            }

            // Incrementar timer
            timerMantenerPresionado += Time.deltaTime;

            // ✅ VERIFICAR SI PUEDE ACTIVAR VISTA VERTICAL
            if (timerMantenerPresionado >= tiempoMantenerPresionado && puedeUsarVistaVertical)
            {
                // Activar cooldown
                if (puedeUsarVistaVertical)
                {
                    puedeUsarVistaVertical = false;
                    timerCooldownVertical = cooldownVistaVertical;

                    if (mostrarDebugCooldown)
                        Debug.Log($"🚀 Vista vertical ACTIVADA - Cooldown iniciado ({cooldownVistaVertical}s)");
                }
            }
        }
        else
        {
            // No hay input, resetear
            if (inputManteniendose)
            {
                inputManteniendose = false;
                timerMantenerPresionado = 0f;

                if (mostrarDebugCooldown)
                    Debug.Log("🛑 Input cancelado - Vista vertical reseteada");
            }
        }
    }

    void LateUpdate()
    {
        if (jugador == null) return;

        ActualizarZonaMuerta();
        ActualizarLookAhead();
        ActualizarVistaVertical();
        ActualizarPosicionCamara();
        AplicarLimites();
    }

    void ActualizarZonaMuerta()
    {
        Vector3 posJugador = jugador.position;
        Vector3 posCamara = transform.position;

        // Calcular límites de zona muerta
        float limiteIzquierdo = centroZonaMuerta.x - anchoZonaMuerta * 0.5f;
        float limiteDerecho = centroZonaMuerta.x + anchoZonaMuerta * 0.5f;
        float limiteInferior = centroZonaMuerta.y - altoZonaMuerta * 0.5f;
        float limiteSuperior = centroZonaMuerta.y + altoZonaMuerta * 0.5f;

        // Verificar si el jugador está fuera de la zona muerta
        Vector3 nuevoCentroZonaMuerta = centroZonaMuerta;

        // Verificación horizontal de zona muerta
        if (posJugador.x < limiteIzquierdo)
        {
            nuevoCentroZonaMuerta.x = posJugador.x + anchoZonaMuerta * 0.5f;
        }
        else if (posJugador.x > limiteDerecho)
        {
            nuevoCentroZonaMuerta.x = posJugador.x - anchoZonaMuerta * 0.5f;
        }

        // Verificación vertical de zona muerta
        if (posJugador.y < limiteInferior)
        {
            nuevoCentroZonaMuerta.y = posJugador.y + altoZonaMuerta * 0.5f;
        }
        else if (posJugador.y > limiteSuperior)
        {
            nuevoCentroZonaMuerta.y = posJugador.y - altoZonaMuerta * 0.5f;
        }

        // Mover suavemente el centro de la zona muerta
        centroZonaMuerta = Vector3.Lerp(centroZonaMuerta, nuevoCentroZonaMuerta, Time.deltaTime * velocidadSeguimiento);
    }

    void ActualizarLookAhead()
    {
        float lookAheadObjetivo = 0f;

        if (rbJugador != null)
        {
            // Usar velocidad para un look ahead más responsivo
            float velocidadHorizontal = rbJugador.velocity.x;

            if (Mathf.Abs(velocidadHorizontal) > umbraLookAhead)
            {
                lookAheadObjetivo = Mathf.Sign(velocidadHorizontal) * distanciaLookAhead;
            }
        }
        else
        {
            // Respaldo: usar rotación del jugador para dirección
            if (jugador.rotation.eulerAngles.y < 90f || jugador.rotation.eulerAngles.y > 270f)
            {
                lookAheadObjetivo = distanciaLookAhead; // Mirando derecha
            }
            else
            {
                lookAheadObjetivo = -distanciaLookAhead; // Mirando izquierda
            }
        }

        lookAheadActual = Mathf.Lerp(lookAheadActual, lookAheadObjetivo, Time.deltaTime * suavidadLookAhead);
    }

    void ActualizarVistaVertical()
    {
        float vistaVerticalObjetivo = 0f;

        // ✅ FUNCIONA DURANTE EL TIEMPO DE MANTENER PRESIONADO + DESPUÉS DEL COOLDOWN
        if (validar_inputs_camara && inputManteniendose &&
            (timerMantenerPresionado >= tiempoMantenerPresionado || !puedeUsarVistaVertical))
        {
            // El input de teclado tiene prioridad
            if (Mathf.Abs(inputCamara) > 0.1f)
            {
                if (inputCamara > 0)
                {
                    vistaVerticalObjetivo = desplazamientoMirarArriba;    // W = cámara arriba
                }
                else
                {
                    vistaVerticalObjetivo = desplazamientoMirarAbajo;     // S = cámara abajo  
                }

                if (mostrarDebugCooldown)
                    Debug.Log($"📹 Vista vertical aplicada: {vistaVerticalObjetivo}");
            }
            // Input del stick derecho del controlador - SIN INVERSIÓN
            else if (Mathf.Abs(inputMirar.y) > 0.1f)
            {
                float inputStick = inputMirar.y;

                if (inputStick > 0)
                {
                    // Stick arriba = cámara arriba
                    vistaVerticalObjetivo = inputStick * desplazamientoMirarArriba;
                }
                else
                {
                    // Stick abajo = cámara abajo
                    vistaVerticalObjetivo = inputStick * Mathf.Abs(desplazamientoMirarAbajo);
                }

                if (mostrarDebugCooldown)
                    Debug.Log($"📹 Vista vertical stick aplicada: {vistaVerticalObjetivo}");
            }
        }

        vistaVerticalActual = Mathf.Lerp(vistaVerticalActual, vistaVerticalObjetivo, Time.deltaTime * velocidadVistaVertical);
    }

    void ActualizarPosicionCamara()
    {
        // Calcular posición objetivo basada en centro de zona muerta + look ahead + vista vertical + sesgo
        posicionObjetivo = new Vector3(
            centroZonaMuerta.x + lookAheadActual,
            centroZonaMuerta.y + vistaVerticalActual + sesgoVertical,
            transform.position.z
        );

        // Movimiento suave de cámara usando SmoothDamp para sensación como Hollow Knight
        transform.position = Vector3.SmoothDamp(transform.position, posicionObjetivo, ref velocidad, amortiguacionSuave);
    }

    void AplicarLimites()
    {
        if (!usarLimitesCamara) return;

        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, limitesMinimos.x, limitesMaximos.x);
        pos.y = Mathf.Clamp(pos.y, limitesMinimos.y, limitesMaximos.y);
        transform.position = pos;
    }

    // 🕒 MÉTODOS PÚBLICOS PARA EL SISTEMA DE COOLDOWN
    public bool EstaEnCooldownVertical()
    {
        return !puedeUsarVistaVertical;
    }

    public float GetCooldownVerticalRestante()
    {
        return Mathf.Max(0f, timerCooldownVertical);
    }

    public void ResetearCooldownVertical()
    {
        timerCooldownVertical = 0f;
        timerMantenerPresionado = 0f;
        puedeUsarVistaVertical = true;
        inputManteniendose = false;

        if (mostrarDebugCooldown)
            Debug.Log("🔄 Cooldown vista vertical reseteado manualmente");
    }

    // Gizmos para visualizar zona muerta en vista de escena
    void OnDrawGizmosSelected()
    {
        if (!mostrarGizmoZonaMuerta) return;

        if (Application.isPlaying)
        {
            // Dibujar zona muerta actual
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(centroZonaMuerta, new Vector3(anchoZonaMuerta, altoZonaMuerta, 0f));

            // Dibujar posición objetivo
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(posicionObjetivo, 0.2f);

            // 🕒 VISUALIZAR ESTADO DEL COOLDOWN
            if (EstaEnCooldownVertical())
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(transform.position + Vector3.up * 2f, Vector3.one * 0.3f);
            }
            else if (inputManteniendose)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(transform.position + Vector3.up * 2f, Vector3.one * 0.3f);
            }
        }
        else if (jugador != null)
        {
            // Dibujar zona muerta inicial en modo edición
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(jugador.position, new Vector3(anchoZonaMuerta, altoZonaMuerta, 0f));
        }
    }

    // Métodos públicos para control externo (como transiciones de habitación)
    public void EstablecerCentroZonaMuerta(Vector3 nuevoCentro)
    {
        centroZonaMuerta = nuevoCentro;
    }

    public void EnfocarEnPosicion(Vector3 puntoEnfoque, float tiempoEnfoque = 1f)
    {
        StartCoroutine(CorrutinaEnfoque(puntoEnfoque, tiempoEnfoque));
    }

    private System.Collections.IEnumerator CorrutinaEnfoque(Vector3 puntoEnfoque, float tiempoEnfoque)
    {
        Vector3 objetivoOriginal = posicionObjetivo;
        float tiempoTranscurrido = 0f;

        while (tiempoTranscurrido < tiempoEnfoque)
        {
            tiempoTranscurrido += Time.deltaTime;
            float t = tiempoTranscurrido / tiempoEnfoque;

            Vector3 objetivoEnfoque = new Vector3(puntoEnfoque.x, puntoEnfoque.y, transform.position.z);
            transform.position = Vector3.Lerp(objetivoOriginal, objetivoEnfoque, t * velocidadEnfoqueRapido);

            yield return null;
        }

        // Resetear zona muerta al nuevo punto de enfoque
        centroZonaMuerta = puntoEnfoque;
    }
}
