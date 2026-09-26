import 'package:flutter/material.dart';

/// Shown briefly while [AuthController.build] resolves the stored session.
/// GoRouter's redirect takes over as soon as that resolves.
class SplashScreen extends StatelessWidget {
  const SplashScreen({super.key});

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Scaffold(
      body: Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Icon(Icons.nightlight_round, size: 64, color: theme.colorScheme.primary),
            const SizedBox(height: 16),
            Text("Gogo's Numbers", style: theme.textTheme.titleLarge),
            const SizedBox(height: 32),
            const CircularProgressIndicator(),
          ],
        ),
      ),
    );
  }
}
