import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';

class OnboardingScreen extends StatelessWidget {
  const OnboardingScreen({super.key});

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Scaffold(
      body: SafeArea(
        child: Padding(
          padding: const EdgeInsets.all(24),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              const Spacer(),
              Icon(Icons.auto_stories, size: 72, color: theme.colorScheme.primary),
              const SizedBox(height: 24),
              Text(
                "Grandma's Dream Numbers",
                textAlign: TextAlign.center,
                style: theme.textTheme.headlineMedium,
              ),
              const SizedBox(height: 16),
              Text(
                'Tell her your dream, and she\'ll find the symbols in it and '
                'the lucky numbers that go with them — the same way she always has.',
                textAlign: TextAlign.center,
                style: theme.textTheme.bodyLarge,
              ),
              const SizedBox(height: 12),
              Text(
                'For entertainment and cultural purposes only.',
                textAlign: TextAlign.center,
                style: theme.textTheme.bodyMedium?.copyWith(
                  color: theme.colorScheme.onSurfaceVariant,
                ),
              ),
              const Spacer(),
              FilledButton(
                onPressed: () => context.go('/login'),
                child: const Padding(
                  padding: EdgeInsets.symmetric(vertical: 12),
                  child: Text('Get started'),
                ),
              ),
              const SizedBox(height: 12),
              TextButton(
                onPressed: () => context.go('/register'),
                child: const Text('Create an account'),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
