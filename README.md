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

# Detalle de Objetos Utilizados en el Proceso de Liquidación

## 1. DenuncioCesantia
Objeto principal que contiene toda la información del siniestro.

```csharp
public class DenuncioCesantia {
    public long Id { get; set; }
    public long NumeroSiniestroReal { get; set; }
    public Cliente Cliente { get; set; }
    public Producto ProductoDenunciado { get; set; }
    public Empleador Empleador { get; set; }
    public DateTime? FechaInicioContrato { get; set; }
    public DateTime? FechaTerminoContrato { get; set; }
    public DateTime FechaDenuncio { get; set; }
    public InfoCausalDenuncio InfoCausalDenuncio { get; set; }
    public LiquidacionCesantia Liquidacion { get; set; }
    public TiposEstadoDenuncioPublico TipoEstadoDenuncioPublico { get; set; }
    public Liquidador Liquidador { get; set; }
    
    // Propiedades adicionales para el formateo
    public string FechaInicioContratoArreglada { get; set; }
    public string FechaTerminoContratoArreglada { get; set; }
    public string CodigoPol { get; set; }
    public string RutaVirtualDocumentoDenuncio { get; set; }
}
```

## 2. LiquidacionCesantia
Contiene la información específica de la liquidación del siniestro.

```csharp
public class LiquidacionCesantia {
    public decimal? PerdidaReclamadaPorCuota { get; set; }
    public List<string> CuotasAPagar { get; set; }
    public decimal? PerdidaDeterminada { get; set; }
    public string GlosaDeducible { get; set; }
    public decimal? MontoIndemnizacion { get; set; }
    public TiposEstadoLiquidacion TiposEstadoLiquidacion { get; set; }
    public string UrlVirtualDocumentoLiquidacion { get; set; }
    public string CausalDenuncioString { get; set; }
    public int? CantidadCuotasCubreSeguro { get; set; }
    public EntidadParametrizable CodigoPoliza { get; set; }
    public long PropietarioId { get; set; }
    
    // Datos para el informe
    public string GlosaAntiguedadMinimaCarencia { get; set; }
    public string GlosaMesesAntiguedadLaboral { get; set; }
    public string GlosaPlazoMaximoCorridosDesde { get; set; }
    public string GlosaPeriodoActivoMinimo { get; set; }
    public string GlosaPosibilidadRecupero { get; set; }
    public string GlosaCuotasAPagar { get; set; }
    public string AntiguedadEnLaPolizaALaFechaSiniestro { get; set; }
    public string AntiguedadEnLaPolizaALaFechaSiniestroDias { get; set; }
    public bool AplicaParrafoProximaCuota { get; set; }
    public string GlosaNumeroProximaCuota { get; set; }
    public string GlosaMesProximaCuota { get; set; }
    public string GlosaMesFechaEmision { get; set; }
    public string DiferenciaFechaContrato { get; set; }
    public string MonedaMontoIndemnizacion { get; set; }
    public string FechaDenuncioPresentable { get; set; }
    public string GlosaTipoInformeLiquidacion { get; set; }
    public bool AplicaParraforLSG { get; set; }
    public string GlosaLSGMesPosteriorDeducible { get; set; }
    public string GlosalitLSGAnioCurso { get; set; }
    public string GlosaCovid { get; set; }
    public string GlosaCovid2 { get; set; }
}
```

## 3. DetalleInforme
Almacena información detallada para el informe de liquidación.

```csharp
public class DetalleInforme {
    public long NumeroSiniestro { get; set; }
    public int AntiguedadMinima { get; set; }
    public int PeriodoActMinima { get; set; }
    public string Carencia { get; set; }
    public string Pol { get; set; }
    
    // Flags para el informe
    public int EsMinimaCarencia { get; set; }
    public int EsMinimoContrato { get; set; }
    public int EsPlazoMaxSiniestro { get; set; }
    public int EsPeriodoActMinima { get; set; }
    public int Articulo159N1 { get; set; }
    public int Articulo159N6 { get; set; }
    public int Articulo161N1 { get; set; }
    public int AceptacionRenuncio { get; set; }
    public int EmpleadoPublico { get; set; }
    public int EmpleadoFFAA { get; set; }
    public int NoImputableFFAA { get; set; }
    public int RetiradoDeBaja { get; set; }
    public string OtroArticulo { get; set; }
    public int PosibleRecupero { get; set; }
    public int DiasMinimaCarencia { get; set; }
    public int DiasPlazoMaxSiniestro { get; set; }
}
```

## 4. PresentadorDocumentoDenuncio
Utilizado para configurar los datos que se presentarán en el informe PDF.

```csharp
public class PresentadorDocumentoDenuncio : PresentadorBaseDocumento {
    public DenuncioCesantia DenuncioInterno { get; set; }
    
    // Flags de configuración para el documento
    public bool AplicaAntiguedadMinimaCarencia { get; set; }
    public bool AplicaAntiguedadMinimaContratacion { get; set; }
    public bool AplicaPlazoMaximoDenuncioSiniestros { get; set; }
    public bool AplicaPeridoActivoMinimo { get; set; }
    
    // Flags para artículos aplicables
    public bool Aplica159N1 { get; set; }
    public bool Aplica159N6 { get; set; }
    public bool Aplica161N1 { get; set; }
    public bool Aplica59N2 { get; set; }
    public bool AplicaAceptacionRenuncia { get; set; }
    public bool AplicaEmpleadosPublicos { get; set; }
    public bool AplicaEmpleadosFFAA { get; set; }
    public bool AplicaCausaNoImputableMiembroFuerzasArmadas { get; set; }
    public bool AplicaIntegranteRetiradoDadoBaja { get; set; }
    public string OtroArticulo { get; set; }
    public bool AplicaOtroArticulo { get; set; }
    
    // Flags para motivos de rechazo
    public bool AplicaRechazoAntiguedadLaboral { get; set; }
    public bool AplicaRechazoA1591 { get; set; }
    public bool AplicaRechazoCreditoNoVigente { get; set; }
    public bool AplicaRechazoCarencia { get; set; }
    public bool AplicaRechazoCausal { get; set; }
    public bool AplicaRechazoEmpleadoPublico { get; set; }
    public bool AplicaRechazoDisconformidadDocumentos { get; set; }
    public bool AplicaRechazoYaIndeminizado { get; set; }
    public bool AplicaRechazoFuerdaPlazo { get; set; }
    public bool AplicaRechazoDependientePresentaIncapacidad { get; set; }
    public bool AplicaRechazoIndependientePresentaCesantia { get; set; }
    public bool AplicaRechazoFaltaInteres { get; set; }
    public bool AplicaRechazoPorNuevasCotizaciones { get; set; }
    public bool AplicaRechazoOtro { get; set; }
    
    // Glosas para los rechazos
    public string GlosaAplicaRechazoAntiguedadLaboral { get; set; }
    public string GlosaAplicaRechazoA1591 { get; set; }
    public string GlosaAplicaRechazoCreditoNoVigente { get; set; }
    public string GlosaAplicaRechazoCarencia { get; set; }
    public string GlosaAplicaRechazoCausal { get; set; }
    public string GlosaAplicaRechazoEmpleadoPublico { get; set; }
    public string GlosaAplicaRechazoDisconformidadDocumentos { get; set; }
    public string GlosaAplicaRechazoYaIndeminizado { get; set; }
    public string GlosaAplicaRechazoFuerdaPlazo { get; set; }
    public string GlosaAplicaRechazoDependientePresentaIncapacidad { get; set; }
    public string GlosaAplicaRechazoIndependientePresentaCesantia { get; set; }
    public string GlosaAplicaRechazoFaltaInteres { get; set; }
    public string GlosaAplicaRechazoPorNuevasCotizaciones { get; set; }
    public string GlosaAplicaRechazoOtro { get; set; }
    public string GlosaRechazoAdicional { get; set; }
    
    // Datos adicionales para nuevas cotizaciones
    public string tbNCCartolaAFP { get; set; }
    public string tbNCCartolaAFPFecha { get; set; }
    public string tbNCRutEmpleador { get; set; }
    public string tbNCRazonEmpleador { get; set; }
    public string tbNCMesDe { get; set; }
    
    // Propiedades para COVID
    public bool AplicaSuspension { get; set; }
    public bool AplicaReduccion { get; set; }
    
    // Datos de formato
    public string NombreFormateadoLiquidador { get; set; }
    public string FechaCompleta { get; set; }
}
```

## 5. Queries y Comandos Principales

### 5.1 Métodos de Consulta

```csharp
public List<DenuncioCesantia> ObtenerDenuncioDetalladoPorIdentificadorGenerado(string denuncioGuidID)
{
    // Obtiene denuncios por su identificador único (GUID)
}

public LiquidacionCesantia ObtenerLiquidacionCesantia(long denuncioId)
{
    // Obtiene datos de liquidación para un denuncio específico
}

public DetalleInforme ObtenerDetalleInforme(long numeroSiniestro)
{
    // Obtiene los detalles del informe para un siniestro
}
```

### 5.2 Métodos de Comando

```csharp
public void ActualizarEstadoDenuncio(DenuncioCesantia denuncio)
{
    // Actualiza el estado del denuncio en la base de datos
}

public void LiquidarDenuncioAceptado(DenuncioCesantia denuncio)
{
    // Registra los datos de liquidación de un denuncio aceptado
}

public void LiquidarDenuncioRechazado(DenuncioCesantia denuncio)
{
    // Registra los datos de liquidación de un denuncio rechazado
}

public void GuardarDetalleInforme(DetalleInforme detalleInforme)
{
    // Guarda los detalles del informe en la base de datos
}
```
