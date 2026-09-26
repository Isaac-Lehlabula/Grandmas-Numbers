import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../../core/network/api_error.dart';
import '../../../core/widgets/skeleton_loader.dart';
import '../application/dream_providers.dart';
import '../data/dream_models.dart';
import '../data/dreams_api.dart';

class DreamHistoryScreen extends ConsumerStatefulWidget {
  const DreamHistoryScreen({super.key});

  @override
  ConsumerState<DreamHistoryScreen> createState() => _DreamHistoryScreenState();
}

class _DreamHistoryScreenState extends ConsumerState<DreamHistoryScreen> {
  String? _query;

  Future<void> _delete(DreamSummary dream) async {
    try {
      await ref.read(dreamsApiProvider).delete(dream.id);
      ref.invalidate(dreamHistoryProvider(_query));
      // Home's "Recent dreams" always reads the null-query cache - refresh
      // it too when a search filter means it wasn't just invalidated above.
      if (_query != null) {
        ref.invalidate(dreamHistoryProvider(null));
      }
    } catch (error) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text(describeApiError(error))),
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    final historyAsync = ref.watch(dreamHistoryProvider(_query));

    return Scaffold(
      appBar: AppBar(title: const Text('Dream history')),
      body: Column(
        children: [
          Padding(
            padding: const EdgeInsets.all(16),
            child: TextField(
              decoration: InputDecoration(
                hintText: 'Search your dreams',
                prefixIcon: const Icon(Icons.search),
                border: OutlineInputBorder(borderRadius: BorderRadius.circular(16)),
              ),
              onChanged: (value) => setState(() => _query = value.trim().isEmpty ? null : value),
            ),
          ),
          Expanded(
            child: historyAsync.when(
              loading: () => const Padding(
                padding: EdgeInsets.symmetric(horizontal: 16),
                child: SkeletonList(count: 5),
              ),
              error: (error, _) => Center(
                child: Padding(
                  padding: const EdgeInsets.all(24),
                  child: Column(
                    mainAxisSize: MainAxisSize.min,
                    children: [
                      Text(describeApiError(error), textAlign: TextAlign.center),
                      const SizedBox(height: 12),
                      FilledButton(
                        onPressed: () => ref.invalidate(dreamHistoryProvider(_query)),
                        child: const Text('Retry'),
                      ),
                    ],
                  ),
                ),
              ),
              data: (dreams) {
                if (dreams.isEmpty) {
                  return Center(
                    child: Text(
                      _query == null ? 'No dreams yet.' : 'No dreams match your search.',
                      style: Theme.of(context).textTheme.bodyLarge,
                    ),
                  );
                }

                return ListView.builder(
                  padding: const EdgeInsets.symmetric(horizontal: 16),
                  itemCount: dreams.length,
                  itemBuilder: (context, index) {
                    final dream = dreams[index];
                    return Card(
                      margin: const EdgeInsets.only(bottom: 8),
                      child: ListTile(
                        title: Text(
                          dream.summary ?? dream.originalText,
                          maxLines: 1,
                          overflow: TextOverflow.ellipsis,
                        ),
                        subtitle: Text(_formatDate(dream.createdAt)),
                        trailing: IconButton(
                          icon: const Icon(Icons.delete_outline),
                          onPressed: () => _delete(dream),
                        ),
                        onTap: () => context.push('/dreams/${dream.id}'),
                      ),
                    );
                  },
                );
              },
            ),
          ),
        ],
      ),
    );
  }

  String _formatDate(DateTime date) =>
      '${date.year}-${date.month.toString().padLeft(2, '0')}-${date.day.toString().padLeft(2, '0')}';
}
