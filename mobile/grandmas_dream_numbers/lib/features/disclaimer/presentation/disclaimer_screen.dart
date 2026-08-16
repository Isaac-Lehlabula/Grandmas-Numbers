import 'package:flutter/material.dart';

class DisclaimerScreen extends StatelessWidget {
  const DisclaimerScreen({super.key});

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Scaffold(
      appBar: AppBar(title: const Text('Disclaimer & responsible use')),
      body: SafeArea(
        child: ListView(
          padding: const EdgeInsets.all(24),
          children: [
            Text('Entertainment only', style: theme.textTheme.titleLarge),
            const SizedBox(height: 8),
            Text(
              "These interpretations are based on traditional dream-number "
              "associations and are provided for entertainment and cultural "
              "purposes only. They do not predict lottery outcomes or "
              "guarantee winnings.",
              style: theme.textTheme.bodyLarge,
            ),
            const SizedBox(height: 24),
            Text('Responsible use', style: theme.textTheme.titleLarge),
            const SizedBox(height: 8),
            Text(
              "Please play responsibly. Never spend more than you can afford "
              "to lose, and don't treat any number in this app as financial "
              "advice or a guaranteed outcome. If gambling stops being fun, "
              "or you feel you can't control it, consider reaching out to a "
              "local support service.",
              style: theme.textTheme.bodyLarge,
            ),
            const SizedBox(height: 24),
            Text('How matching works', style: theme.textTheme.titleLarge),
            const SizedBox(height: 8),
            Text(
              "Every number you see comes from a curated database of "
              "traditional symbol-to-number associations. An AI helps "
              "identify which symbols appear in your dream, but it can "
              "never invent a number - only numbers already stored for a "
              "matched symbol are ever shown to you.",
              style: theme.textTheme.bodyLarge,
            ),
          ],
        ),
      ),
    );
  }
}
