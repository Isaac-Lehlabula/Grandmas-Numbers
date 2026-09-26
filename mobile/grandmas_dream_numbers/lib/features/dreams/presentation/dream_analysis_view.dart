import 'package:flutter/material.dart';

import '../data/dream_models.dart';

/// Shared rendering for an analysis result - used by both the fresh
/// "results" screen and the "details" screen when reopening a past dream.
class DreamAnalysisView extends StatelessWidget {
  const DreamAnalysisView({super.key, required this.result});

  final DreamAnalysisResult result;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return ListView(
      padding: const EdgeInsets.all(24),
      children: [
        Text('Your dream', style: theme.textTheme.titleLarge),
        const SizedBox(height: 8),
        Text(result.originalText, style: theme.textTheme.bodyLarge),
        const SizedBox(height: 24),
        Text('What Gogo sees', style: theme.textTheme.titleLarge),
        const SizedBox(height: 8),
        Text(result.summary, style: theme.textTheme.bodyLarge),
        const SizedBox(height: 24),
        if (result.matches.isEmpty)
          _EmptyMatchesCard(theme: theme)
        else ...[
          Text('Symbols found', style: theme.textTheme.titleLarge),
          const SizedBox(height: 12),
          ...result.matches.map((match) => _SymbolMatchCard(match: match)),
        ],
        if (result.suggestedCombinations.isNotEmpty) ...[
          const SizedBox(height: 24),
          Text('Suggested combinations', style: theme.textTheme.titleLarge),
          const SizedBox(height: 12),
          ...result.suggestedCombinations.map(
            (combo) => _CombinationRow(combination: combo),
          ),
        ],
        const SizedBox(height: 24),
        Container(
          padding: const EdgeInsets.all(16),
          decoration: BoxDecoration(
            color: theme.colorScheme.surfaceContainerHighest,
            borderRadius: BorderRadius.circular(16),
          ),
          child: Text(
            result.disclaimer,
            style: theme.textTheme.bodyMedium?.copyWith(
              color: theme.colorScheme.onSurfaceVariant,
            ),
          ),
        ),
      ],
    );
  }
}

class _EmptyMatchesCard extends StatelessWidget {
  const _EmptyMatchesCard({required this.theme});

  final ThemeData theme;

  @override
  Widget build(BuildContext context) {
    return Card(
      elevation: 0,
      color: theme.colorScheme.surfaceContainerHighest,
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
      child: const Padding(
        padding: EdgeInsets.all(20),
        child: Text('Gogo didn\'t recognize a symbol in this dream this time.'),
      ),
    );
  }
}

class _SymbolMatchCard extends StatelessWidget {
  const _SymbolMatchCard({required this.match});

  final DreamSymbolMatch match;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Card(
      margin: const EdgeInsets.only(bottom: 12),
      elevation: 0,
      color: theme.colorScheme.surfaceContainerHighest,
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(match.symbolName, style: theme.textTheme.titleLarge),
            const SizedBox(height: 4),
            Text(match.traditionalMeaning, style: theme.textTheme.bodyMedium),
            const SizedBox(height: 8),
            Text(match.explanation, style: theme.textTheme.bodyMedium),
            const SizedBox(height: 12),
            Wrap(
              spacing: 8,
              children: match.luckyNumbers
                  .map((n) => Chip(label: Text('$n'), backgroundColor: theme.colorScheme.primaryContainer))
                  .toList(),
            ),
          ],
        ),
      ),
    );
  }
}

class _CombinationRow extends StatelessWidget {
  const _CombinationRow({required this.combination});

  final SuggestedCombination combination;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Padding(
      padding: const EdgeInsets.only(bottom: 8),
      child: Row(
        children: combination.numbers
            .map(
              (n) => Padding(
                padding: const EdgeInsets.only(right: 8),
                child: CircleAvatar(
                  backgroundColor: theme.colorScheme.primary,
                  foregroundColor: theme.colorScheme.onPrimary,
                  child: Text('$n'),
                ),
              ),
            )
            .toList(),
      ),
    );
  }
}
