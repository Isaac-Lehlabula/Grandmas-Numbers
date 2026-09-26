import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';

class ReviewTranscriptScreen extends StatefulWidget {
  const ReviewTranscriptScreen({super.key, required this.initialTranscript});

  final String initialTranscript;

  @override
  State<ReviewTranscriptScreen> createState() => _ReviewTranscriptScreenState();
}

class _ReviewTranscriptScreenState extends State<ReviewTranscriptScreen> {
  late final TextEditingController _controller;

  @override
  void initState() {
    super.initState();
    _controller = TextEditingController(text: widget.initialTranscript);
  }

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  void _analyse() {
    final text = _controller.text.trim();
    if (text.isEmpty) {
      return;
    }
    context.push('/dreams/analysing', extra: text);
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Review your dream')),
      body: SafeArea(
        child: Padding(
          padding: const EdgeInsets.all(24),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              Text(
                "Make any changes before sending this to Gogo.",
                style: Theme.of(context).textTheme.bodyMedium,
              ),
              const SizedBox(height: 16),
              Expanded(
                child: TextField(
                  controller: _controller,
                  maxLines: null,
                  expands: true,
                  textAlignVertical: TextAlignVertical.top,
                  decoration: InputDecoration(
                    hintText: 'Describe your dream...',
                    border: OutlineInputBorder(borderRadius: BorderRadius.circular(16)),
                  ),
                ),
              ),
              const SizedBox(height: 16),
              FilledButton(
                onPressed: _analyse,
                child: const Padding(
                  padding: EdgeInsets.symmetric(vertical: 12),
                  child: Text('Analyse my dream'),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
