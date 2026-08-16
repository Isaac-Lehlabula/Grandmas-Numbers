import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../../core/network/api_error.dart';
import '../application/auth_controller.dart';

class LoginScreen extends ConsumerStatefulWidget {
  const LoginScreen({super.key});

  @override
  ConsumerState<LoginScreen> createState() => _LoginScreenState();
}

class _LoginScreenState extends ConsumerState<LoginScreen> {
  final _formKey = GlobalKey<FormState>();
  final _emailController = TextEditingController();
  final _passwordController = TextEditingController();

  @override
  void dispose() {
    _emailController.dispose();
    _passwordController.dispose();
    super.dispose();
  }

  Future<void> _submit() async {
    if (!_formKey.currentState!.validate()) {
      return;
    }

    await ref
        .read(authControllerProvider.notifier)
        .login(email: _emailController.text.trim(), password: _passwordController.text);
  }

  @override
  Widget build(BuildContext context) {
    final authState = ref.watch(authControllerProvider);
    final isLoading = authState.isLoading;

    return Scaffold(
      appBar: AppBar(title: const Text('Welcome back')),
      body: SafeArea(
        child: SingleChildScrollView(
          padding: const EdgeInsets.all(24),
          child: Form(
            key: _formKey,
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                TextFormField(
                  controller: _emailController,
                  keyboardType: TextInputType.emailAddress,
                  decoration: const InputDecoration(labelText: 'Email'),
                  validator: (value) =>
                      (value == null || value.trim().isEmpty) ? 'Enter your email' : null,
                ),
                const SizedBox(height: 16),
                TextFormField(
                  controller: _passwordController,
                  obscureText: true,
                  decoration: const InputDecoration(labelText: 'Password'),
                  validator: (value) =>
                      (value == null || value.isEmpty) ? 'Enter your password' : null,
                ),
                if (authState.hasError) ...[
                  const SizedBox(height: 16),
                  Text(
                    describeApiError(authState.error!),
                    style: TextStyle(color: Theme.of(context).colorScheme.error),
                  ),
                ],
                const SizedBox(height: 24),
                FilledButton(
                  onPressed: isLoading ? null : _submit,
                  child: Padding(
                    padding: const EdgeInsets.symmetric(vertical: 12),
                    child: isLoading
                        ? const SizedBox(
                            height: 20,
                            width: 20,
                            child: CircularProgressIndicator(strokeWidth: 2),
                          )
                        : const Text('Log in'),
                  ),
                ),
                const SizedBox(height: 12),
                TextButton(
                  onPressed: () => context.go('/register'),
                  child: const Text("Don't have an account? Register"),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}
