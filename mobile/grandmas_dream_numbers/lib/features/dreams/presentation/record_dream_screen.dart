import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:speech_to_text/speech_to_text.dart';

class RecordDreamScreen extends StatefulWidget {
  const RecordDreamScreen({super.key});

  @override
  State<RecordDreamScreen> createState() => _RecordDreamScreenState();
}

class _RecordDreamScreenState extends State<RecordDreamScreen> {
  final SpeechToText _speech = SpeechToText();
  bool _isListening = false;
  bool _speechAvailable = false;
  bool _speechInitFailed = false;
  String _transcript = '';

  @override
  void initState() {
    super.initState();
    _initSpeech();
  }

  Future<void> _initSpeech() async {
    try {
      final available = await _speech.initialize(
        onStatus: (status) {
          if (status == 'done' || status == 'notListening') {
            setState(() => _isListening = false);
          }
        },
        onError: (_) => setState(() => _isListening = false),
      );
      setState(() {
        _speechAvailable = available;
        _speechInitFailed = !available;
      });
    } catch (_) {
      setState(() => _speechInitFailed = true);
    }
  }

  Future<void> _toggleListening() async {
    if (_isListening) {
      await _speech.stop();
      setState(() => _isListening = false);
      return;
    }

    setState(() => _isListening = true);
    await _speech.listen(
      onResult: (result) => setState(() => _transcript = result.recognizedWords),
    );
  }

  void _continue() {
    context.push('/dreams/review', extra: _transcript);
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Scaffold(
      appBar: AppBar(title: const Text('Record your dream')),
      body: SafeArea(
        child: Padding(
          padding: const EdgeInsets.all(24),
          child: Column(
            children: [
              const Spacer(),
              Text(
                _isListening ? "I'm listening..." : 'Tap the microphone to start',
                style: theme.textTheme.titleLarge,
                textAlign: TextAlign.center,
              ),
              const SizedBox(height: 16),
              Expanded(
                child: SingleChildScrollView(
                  child: Text(
                    _transcript.isEmpty ? 'Your words will appear here.' : _transcript,
                    textAlign: TextAlign.center,
                    style: theme.textTheme.bodyLarge?.copyWith(
                      color: _transcript.isEmpty ? theme.colorScheme.onSurfaceVariant : null,
                    ),
                  ),
                ),
              ),
              if (_speechInitFailed)
                Padding(
                  padding: const EdgeInsets.only(bottom: 16),
                  child: Text(
                    "Voice capture isn't available on this device — you can type your dream instead.",
                    textAlign: TextAlign.center,
                    style: TextStyle(color: theme.colorScheme.error),
                  ),
                ),
              SizedBox(
                width: 96,
                height: 96,
                child: FloatingActionButton(
                  heroTag: 'record-dream',
                  onPressed: _speechAvailable ? _toggleListening : null,
                  backgroundColor: _isListening ? theme.colorScheme.error : null,
                  child: Icon(_isListening ? Icons.stop : Icons.mic, size: 40),
                ),
              ),
              const SizedBox(height: 24),
              FilledButton(
                onPressed: _transcript.trim().isEmpty ? null : _continue,
                child: const Padding(
                  padding: EdgeInsets.symmetric(vertical: 12),
                  child: Text('Continue'),
                ),
              ),
              TextButton(
                onPressed: () => context.push('/dreams/review', extra: ''),
                child: const Text('Type my dream instead'),
              ),
            ],
          ),
        ),
      ),
    );
  }

  @override
  void dispose() {
    _speech.stop();
    super.dispose();
  }
}
