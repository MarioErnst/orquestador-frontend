import 'package:flutter/material.dart';

// Provisional design tokens. The definitive ACHS visual identity is pending;
// when it arrives, only this file must change.

class AppTokens {
  AppTokens._();

  // Corporate deep blue, used as Material 3 ColorScheme seed.
  static const Color primarySeed = Color(0xFF003D7C);

  // Sober status palette. Always paired with text or icon, never color-only.
  static const Color statusOk = Color(0xFF1F7A4F);
  static const Color statusWarn = Color(0xFFB7791F);
  static const Color statusAlert = Color(0xFFB42318);

  static const double radius = 12.0;
  static const double spacing = 16.0;
}

class AppTheme {
  AppTheme._();

  static ThemeData light() {
    final scheme = ColorScheme.fromSeed(
      seedColor: AppTokens.primarySeed,
      brightness: Brightness.light,
    );
    return _build(scheme);
  }

  static ThemeData dark() {
    final scheme = ColorScheme.fromSeed(
      seedColor: AppTokens.primarySeed,
      brightness: Brightness.dark,
    );
    return _build(scheme);
  }

  static ThemeData _build(ColorScheme scheme) {
    return ThemeData(
      colorScheme: scheme,
      useMaterial3: true,
      visualDensity: VisualDensity.adaptivePlatformDensity,
      cardTheme: CardThemeData(
        elevation: 0,
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(AppTokens.radius),
          side: BorderSide(color: scheme.outlineVariant),
        ),
      ),
      filledButtonTheme: FilledButtonThemeData(
        style: FilledButton.styleFrom(
          minimumSize: const Size(44, 44),
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(AppTokens.radius),
          ),
        ),
      ),
      outlinedButtonTheme: OutlinedButtonThemeData(
        style: OutlinedButton.styleFrom(
          minimumSize: const Size(44, 44),
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(AppTokens.radius),
          ),
        ),
      ),
    );
  }
}
