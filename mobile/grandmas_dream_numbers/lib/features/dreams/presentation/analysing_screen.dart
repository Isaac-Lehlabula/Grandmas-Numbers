import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../../core/network/api_error.dart';
import '../application/dream_providers.dart';

class AnalysingScreen extends ConsumerWidget {
  const AnalysingScreen({super.key, required this.dreamText});

  final String dreamText;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final submission = ref.watch(dreamSubmissionProvider(dreamText));

    return Scaffold(
      body: SafeArea(
        child: Center(
          child: submission.when(
            loading: () => const _AnalysingBody(),
            data: (result) {
              WidgetsBinding.instance.addPostFrameCallback((_) {
                // Home and History stay mounted (HomeShell uses an
                // IndexedStack), so their dreamHistoryProvider(null) cache
                // never naturally refreshes on its own - without this, a
                // freshly submitted dream wouldn't show up in either until
                // the app restarted.
                ref.invalidate(dreamHistoryProvider(null));
                if (context.mounted) {
                  context.pushReplacement('/dreams/results', extra: result);
                }
              });
              return const _AnalysingBody();
            },
            error: (error, _) => _ErrorBody(
              message: describeApiError(error),
              onRetry: () => ref.invalidate(dreamSubmissionProvider(dreamText)),
            ),
          ),
        ),
      ),
    );
  }
}

class _AnalysingBody extends StatelessWidget {
  const _AnalysingBody();

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Padding(
      padding: const EdgeInsets.all(24),
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          const CircularProgressIndicator(),
          const SizedBox(height: 24),
          Text('Gogo is thinking about your dream...', style: theme.textTheme.titleMedium),
        ],
      ),
    );
  }
}

class _ErrorBody extends StatelessWidget {
  const _ErrorBody({required this.message, required this.onRetry});

  final String message;
  final VoidCallback onRetry;

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.all(24),
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(Icons.error_outline, size: 48, color: Theme.of(context).colorScheme.error),
          const SizedBox(height: 16),
          Text(message, textAlign: TextAlign.center),
          const SizedBox(height: 16),
          FilledButton(onPressed: onRetry, child: const Text('Try again')),
        ],
      ),
    );
  }
}
