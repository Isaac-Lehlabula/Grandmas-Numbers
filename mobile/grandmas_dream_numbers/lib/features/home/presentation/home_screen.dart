import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../../core/network/api_error.dart';
import '../../../core/widgets/skeleton_loader.dart';
import '../../dreams/application/dream_providers.dart';

/// Home tab: the primary entry point for starting a new dream.
class HomeScreen extends ConsumerWidget {
  const HomeScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final theme = Theme.of(context);
    final recentDreams = ref.watch(dreamHistoryProvider(null));

    return Scaffold(
      body: SafeArea(
        child: SingleChildScrollView(
          padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 16),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              const SizedBox(height: 24),
              Text(
                'Tell me your dream',
                textAlign: TextAlign.center,
                style: theme.textTheme.headlineMedium,
              ),
              const SizedBox(height: 32),
              Center(
                child: SizedBox(
                  width: 96,
                  height: 96,
                  child: FloatingActionButton(
                    heroTag: 'record-dream',
                    onPressed: () => context.push('/dreams/record'),
                    child: const Icon(Icons.mic, size: 40),
                  ),
                ),
              ),
              const SizedBox(height: 24),
              OutlinedButton(
                onPressed: () => context.push('/dreams/review', extra: ''),
                child: const Padding(
                  padding: EdgeInsets.symmetric(vertical: 12),
                  child: Text('Type it instead'),
                ),
              ),
              const SizedBox(height: 32),
              Text('Recent dreams', style: theme.textTheme.titleLarge),
              const SizedBox(height: 12),
              recentDreams.when(
                loading: () => const SkeletonList(count: 2),
                error: (error, _) => _PlaceholderCard(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(describeApiError(error)),
                      const SizedBox(height: 8),
                      TextButton(
                        onPressed: () => ref.invalidate(dreamHistoryProvider(null)),
                        child: const Text('Retry'),
                      ),
                    ],
                  ),
                ),
                data: (dreams) {
                  if (dreams.isEmpty) {
                    return _PlaceholderCard(
                      child: Text(
                        'No dreams yet. Your history will appear here.',
                        style: theme.textTheme.bodyMedium,
                      ),
                    );
                  }

                  final recent = dreams.take(3).toList();
                  return Column(
                    children: recent
                        .map(
                          (dream) => Card(
                            margin: const EdgeInsets.only(bottom: 8),
                            child: ListTile(
                              title: Text(
                                dream.summary ?? dream.originalText,
                                maxLines: 1,
                                overflow: TextOverflow.ellipsis,
                              ),
                              onTap: () => context.push('/dreams/${dream.id}'),
                            ),
                          ),
                        )
                        .toList(),
                  );
                },
              ),
              const SizedBox(height: 24),
              Text("Today's symbol", style: theme.textTheme.titleLarge),
              const SizedBox(height: 12),
              _PlaceholderCard(
                child: Text('Coming soon', style: theme.textTheme.bodyMedium),
              ),
            ],
          ),
        ),
      ),
    );
  }
}

class _PlaceholderCard extends StatelessWidget {
  const _PlaceholderCard({required this.child});

  final Widget child;

  @override
  Widget build(BuildContext context) {
    return Card(
      elevation: 0,
      color: Theme.of(context).colorScheme.surfaceContainerHighest,
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
      child: Padding(padding: const EdgeInsets.all(20), child: child),
    );
  }
}
