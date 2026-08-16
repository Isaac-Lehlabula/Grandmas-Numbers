import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../../core/theme/theme_mode_provider.dart';
import '../../auth/application/auth_controller.dart';

class SettingsScreen extends ConsumerWidget {
  const SettingsScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final themeMode = ref.watch(themeModeProvider);

    return Scaffold(
      appBar: AppBar(title: const Text('Settings')),
      body: SafeArea(
        child: ListView(
          children: [
            ListTile(
              title: const Text('Appearance'),
              subtitle: Text(_labelFor(themeMode)),
            ),
            RadioGroup<ThemeMode>(
              groupValue: themeMode,
              onChanged: (mode) => ref.read(themeModeProvider.notifier).state = mode!,
              child: const Column(
                children: [
                  RadioListTile<ThemeMode>(title: Text('Match system'), value: ThemeMode.system),
                  RadioListTile<ThemeMode>(title: Text('Light'), value: ThemeMode.light),
                  RadioListTile<ThemeMode>(title: Text('Dark'), value: ThemeMode.dark),
                ],
              ),
            ),
            const Divider(),
            ListTile(
              leading: const Icon(Icons.info_outline),
              title: const Text('Disclaimer & responsible use'),
              trailing: const Icon(Icons.chevron_right),
              onTap: () => context.push('/disclaimer'),
            ),
            ListTile(
              leading: const Icon(Icons.logout),
              title: const Text('Log out'),
              onTap: () => ref.read(authControllerProvider.notifier).logout(),
            ),
          ],
        ),
      ),
    );
  }

  String _labelFor(ThemeMode mode) => switch (mode) {
    ThemeMode.system => 'Matching system',
    ThemeMode.light => 'Light',
    ThemeMode.dark => 'Dark',
  };
}
