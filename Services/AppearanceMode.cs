namespace OrquestadorFrontend.Services;

// User preference for the application appearance. System mirrors the OS
// setting (and reacts to it live); Light and Dark force one scheme
// regardless of what the OS reports. Persisted as the enum name string.
public enum AppearanceMode
{
    System,
    Light,
    Dark,
}
