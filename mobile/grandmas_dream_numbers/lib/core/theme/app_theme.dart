import 'package:flutter/material.dart';

/// Warm, culturally-rooted theming for the app. Kept centralized so the
/// "grandmother's kitchen table" feel stays consistent across screens.
abstract final class AppTheme {
  static const _seedColor = Color(0xFF8D5B3F);

  static ThemeData light() {
    final colorScheme = ColorScheme.fromSeed(
      seedColor: _seedColor,
      brightness: Brightness.light,
    );

    return ThemeData(
      useMaterial3: true,
      colorScheme: colorScheme,
      scaffoldBackgroundColor: colorScheme.surface,
      textTheme: _textTheme,
      appBarTheme: AppBarTheme(
        backgroundColor: colorScheme.surface,
        foregroundColor: colorScheme.onSurface,
        elevation: 0,
        centerTitle: true,
      ),
    );
  }

  static ThemeData dark() {
    final colorScheme = ColorScheme.fromSeed(
      seedColor: _seedColor,
      brightness: Brightness.dark,
    );

    return ThemeData(
      useMaterial3: true,
      colorScheme: colorScheme,
      scaffoldBackgroundColor: colorScheme.surface,
      textTheme: _textTheme,
      appBarTheme: AppBarTheme(
        backgroundColor: colorScheme.surface,
        foregroundColor: colorScheme.onSurface,
        elevation: 0,
        centerTitle: true,
      ),
    );
  }

  // Large, accessible defaults per the mobile UX requirements.
  static const _textTheme = TextTheme(
    headlineMedium: TextStyle(fontSize: 28, fontWeight: FontWeight.w600),
    titleLarge: TextStyle(fontSize: 22, fontWeight: FontWeight.w600),
    bodyLarge: TextStyle(fontSize: 17),
    bodyMedium: TextStyle(fontSize: 15),
  );
}
