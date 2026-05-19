// Roles supported by the ACHS Directors app. Names match the Propuesta 2
// inventory and stay stable across the data layer and the UI.

enum UserRole {
  director,
  comite,
  altaGerencia,
}

extension UserRoleX on UserRole {
  String get label {
    switch (this) {
      case UserRole.director:
        return 'Director';
      case UserRole.comite:
        return 'Comité';
      case UserRole.altaGerencia:
        return 'Alta Gerencia';
    }
  }
}
