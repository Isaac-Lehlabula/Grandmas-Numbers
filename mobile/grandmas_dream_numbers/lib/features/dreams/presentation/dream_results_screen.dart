import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:go_router/go_router.dart';

import '../data/dream_models.dart';
import 'dream_analysis_view.dart';

class DreamResultsScreen extends StatelessWidget {
  const DreamResultsScreen({super.key, required this.result});

  final DreamAnalysisResult result;

  void _share(BuildContext context) {
    final numbers = result.suggestedCombinations.map((c) => c.numbers.join(', ')).join(' | ');
    final text = '${result.summary}\n\nSuggested numbers: $numbers\n\n${result.disclaimer}';
    Clipboard.setData(ClipboardData(text: text));
    ScaffoldMessenger.of(context).showSnackBar(
      const SnackBar(content: Text('Copied to clipboard')),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Your dream, interpreted'),
        actions: [
          IconButton(
            icon: const Icon(Icons.share_outlined),
            onPressed: () => _share(context),
            tooltip: 'Share',
          ),
        ],
      ),
      body: DreamAnalysisView(result: result),
      bottomNavigationBar: SafeArea(
        child: Padding(
          padding: const EdgeInsets.all(16),
          child: FilledButton(
            onPressed: () => context.go('/home'),
            child: const Padding(
              padding: EdgeInsets.symmetric(vertical: 12),
              child: Text('Done'),
            ),
          ),
        ),
      ),
    );
  }
}
