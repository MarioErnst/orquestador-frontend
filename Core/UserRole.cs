namespace OrquestadorFrontend.Core;

// The three corporate roles defined by Microsoft Entra ID in Propuesta 1.
// They live as a flat enum because the prototype does not yet resolve
// permissions from the platform identity layer.
public enum UserRole
{
    Director,
    Comite,
    AltaGerencia
}
