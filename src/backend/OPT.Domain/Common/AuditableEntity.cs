namespace OPT.Domain.Common;

/// <summary>
/// Base para toda entidad de negocio que requiere auditoría y borrado lógico.
/// Toda tabla transaccional hereda de esta clase — no es opcional (ADR 0003).
/// </summary>
public abstract class AuditableEntity
{
    /// <summary>Identificador sintético generado por la base de datos (IDENTITY).</summary>
    public int Id { get; protected set; }

    public DateTimeOffset CreadoEn  { get; private set; }
    public int            CreadoPor { get; private set; }   // FK a Usuario.Id
    public DateTimeOffset? ModificadoEn  { get; private set; }
    public int?            ModificadoPor { get; private set; }

    /// <summary>Borrado lógico — nunca DELETE físico en tablas de negocio (ADR 0003).</summary>
    public bool Eliminado   { get; private set; }
    public DateTimeOffset? EliminadoEn  { get; private set; }
    public int?            EliminadoPor { get; private set; }

    protected void SetCreacion(int usuarioId)
    {
        CreadoEn  = DateTimeOffset.UtcNow;
        CreadoPor = usuarioId;
    }

    protected void SetModificacion(int usuarioId)
    {
        ModificadoEn  = DateTimeOffset.UtcNow;
        ModificadoPor = usuarioId;
    }

    public void Eliminar(int usuarioId)
    {
        Eliminado    = true;
        EliminadoEn  = DateTimeOffset.UtcNow;
        EliminadoPor = usuarioId;
    }
}
