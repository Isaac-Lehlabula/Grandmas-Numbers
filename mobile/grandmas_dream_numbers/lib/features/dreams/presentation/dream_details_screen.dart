import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../../core/network/api_error.dart';
import '../../../core/widgets/skeleton_loader.dart';
import '../application/dream_providers.dart';
import 'dream_analysis_view.dart';

class DreamDetailsScreen extends ConsumerWidget {
  const DreamDetailsScreen({super.key, required this.dreamId});

  final String dreamId;

  Future<void> _analyseAgain(BuildContext context, WidgetRef ref) async {
    try {
      final result = await ref.read(dreamReanalysisProvider(dreamId).future);
      if (context.mounted) {
        context.push('/dreams/results', extra: result);
      }
    } catch (error) {
      if (context.mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text(describeApiError(error))),
        );
      }
    }
  }

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final detailAsync = ref.watch(dreamDetailProvider(dreamId));

    return Scaffold(
      appBar: AppBar(
        title: const Text('Dream details'),
        actions: [
          IconButton(
            icon: const Icon(Icons.refresh),
            tooltip: 'Analyse again',
            onPressed: () => _analyseAgain(context, ref),
          ),
        ],
      ),
      body: detailAsync.when(
        loading: () => const SkeletonDreamAnalysis(),
        error: (error, _) => Center(
          child: Padding(
            padding: const EdgeInsets.all(24),
            child: Column(
              mainAxisSize: MainAxisSize.min,
              children: [
                Text(describeApiError(error), textAlign: TextAlign.center),
                const SizedBox(height: 12),
                FilledButton(
                  onPressed: () => ref.invalidate(dreamDetailProvider(dreamId)),
                  child: const Text('Retry'),
                ),
              ],
            ),
          ),
        ),
        data: (result) => DreamAnalysisView(result: result),
      ),
    );
  }
}
