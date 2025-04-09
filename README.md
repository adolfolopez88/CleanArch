```mermaid
flowchart TD
    A[Usuario hace clic en 'Generar Vista Previa'] --> B{¿Es Vista Previa?}
    B -- Sí --> C[generarDocumento(true)]
    B -- No --> D[generarDocumento(false)]
    
    C --> E[LeerFormulario]
    D --> E
    
    E --> F[generarCarpetaDocumentos]
    F --> G[Preparar datos de liquidación]
    
    G --> H{¿Siniestro Aceptado?}
    
    H -- Sí --> I[ConfigurarDocumentoAceptado]
    H -- No --> J[ConfigurarDocumentoRechazado]
    
    I --> K[generarDocumentoLiquidacionRetornandoRutaArchivo]
    J --> K
    
    K --> L[Configurar nombre archivo]
    L --> M[Preparar datos denuncio]
    M --> N[Configurar presentador documento]
    N --> O[Aplicar transformación XSLT]
    O --> P[Crear archivo PDF]
    
    P --> Q{¿Es Vista Previa?}
    
    Q -- Sí --> R[Mostrar link a documento]
    Q -- No --> S[Guardar documento y actualizar estado]
    
    S --> T{¿Enviar por correo?}
    T -- Sí --> U[Enviar correos a cliente/ejecutivo]
    T -- No --> V[Terminar proceso]
    
    U --> V
    R --> V
```
