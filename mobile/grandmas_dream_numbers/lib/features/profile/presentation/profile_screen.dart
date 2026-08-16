import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../auth/application/auth_controller.dart';

class ProfileScreen extends ConsumerWidget {
  const ProfileScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final theme = Theme.of(context);
    final authState = ref.watch(authControllerProvider);
    final profile = authState.valueOrNull;

    return Scaffold(
      appBar: AppBar(title: const Text('Profile')),
      body: SafeArea(
        child: Padding(
          padding: const EdgeInsets.all(24),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              CircleAvatar(
                radius: 40,
                backgroundColor: theme.colorScheme.primaryContainer,
                child: Text(
                  profile == null || profile.firstName.isEmpty
                      ? '?'
                      : profile.firstName[0].toUpperCase(),
                  style: theme.textTheme.headlineMedium,
                ),
              ),
              const SizedBox(height: 16),
              Center(
                child: Text(profile?.firstName ?? '', style: theme.textTheme.titleLarge),
              ),
              Center(
                child: Text(
                  profile?.email ?? '',
                  style: theme.textTheme.bodyMedium?.copyWith(
                    color: theme.colorScheme.onSurfaceVariant,
                  ),
                ),
              ),
              const SizedBox(height: 32),
              ListTile(
                leading: const Icon(Icons.settings_outlined),
                title: const Text('Settings'),
                trailing: const Icon(Icons.chevron_right),
                onTap: () => context.push('/settings'),
              ),
              ListTile(
                leading: const Icon(Icons.info_outline),
                title: const Text('Disclaimer & responsible use'),
                trailing: const Icon(Icons.chevron_right),
                onTap: () => context.push('/disclaimer'),
              ),
              const Spacer(),
              OutlinedButton(
                onPressed: () => ref.read(authControllerProvider.notifier).logout(),
                child: const Padding(
                  padding: EdgeInsets.symmetric(vertical: 12),
                  child: Text('Log out'),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
