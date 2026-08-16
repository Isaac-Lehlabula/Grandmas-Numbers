import 'package:flutter/material.dart';

/// A gentle pulsing placeholder box shown while content loads. Deliberately
/// subtle - a slow opacity fade, not a shimmer sweep - to stay in line with
/// the "no excessive animations" guidance.
class SkeletonBox extends StatefulWidget {
  const SkeletonBox({super.key, this.height = 16, this.width, this.borderRadius = 8});

  final double height;
  final double? width;
  final double borderRadius;

  @override
  State<SkeletonBox> createState() => _SkeletonBoxState();
}

class _SkeletonBoxState extends State<SkeletonBox> with SingleTickerProviderStateMixin {
  late final AnimationController _controller;
  late final Animation<double> _opacity;

  @override
  void initState() {
    super.initState();
    _controller = AnimationController(vsync: this, duration: const Duration(milliseconds: 1000))
      ..repeat(reverse: true);
    _opacity = Tween<double>(
      begin: 0.4,
      end: 0.9,
    ).animate(CurvedAnimation(parent: _controller, curve: Curves.easeInOut));
  }

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final baseColor = Theme.of(context).colorScheme.surfaceContainerHighest;

    return AnimatedBuilder(
      animation: _opacity,
      builder: (context, child) => Opacity(
        opacity: _opacity.value,
        child: Container(
          height: widget.height,
          width: widget.width,
          decoration: BoxDecoration(
            color: baseColor,
            borderRadius: BorderRadius.circular(widget.borderRadius),
          ),
        ),
      ),
    );
  }
}

/// Placeholder shaped like a dream-history list card.
class SkeletonListTile extends StatelessWidget {
  const SkeletonListTile({super.key});

  @override
  Widget build(BuildContext context) {
    return Card(
      margin: const EdgeInsets.only(bottom: 8),
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            const SkeletonBox(height: 16, width: 180),
            const SizedBox(height: 8),
            const SkeletonBox(height: 12, width: 90),
          ],
        ),
      ),
    );
  }
}

/// A short list of [SkeletonListTile]s, for the Home/History list-loading states.
class SkeletonList extends StatelessWidget {
  const SkeletonList({super.key, this.count = 3});

  final int count;

  @override
  Widget build(BuildContext context) {
    return Column(
      children: List.generate(count, (_) => const SkeletonListTile()),
    );
  }
}

/// Placeholder shaped like [DreamAnalysisView] - used while a dream's full
/// analysis is loading (e.g. opening a past dream's details).
class SkeletonDreamAnalysis extends StatelessWidget {
  const SkeletonDreamAnalysis({super.key});

  @override
  Widget build(BuildContext context) {
    return ListView(
      padding: const EdgeInsets.all(24),
      children: [
        const SkeletonBox(height: 14, width: 100),
        const SizedBox(height: 8),
        const SkeletonBox(height: 16),
        const SizedBox(height: 4),
        const SkeletonBox(height: 16, width: 220),
        const SizedBox(height: 24),
        const SkeletonBox(height: 14, width: 130),
        const SizedBox(height: 12),
        const SkeletonListTile(),
        const SkeletonListTile(),
      ],
    );
  }
}
