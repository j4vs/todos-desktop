namespace Todos.Domain;

public sealed class UnsupportedSchemaVersionException(int schemaVersion)
    : Exception($"El archivo de Todo's usa la versión {schemaVersion}, que no es compatible con esta aplicación.")
{
    public int SchemaVersion { get; } = schemaVersion;
}
