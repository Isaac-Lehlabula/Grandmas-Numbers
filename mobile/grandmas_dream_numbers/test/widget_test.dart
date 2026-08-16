import 'dart:async';

import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';

import 'package:grandmas_dream_numbers/app.dart';
import 'package:grandmas_dream_numbers/features/auth/application/auth_controller.dart';
import 'package:grandmas_dream_numbers/features/auth/data/auth_models.dart';

/// Never resolves, so the app stays on the splash screen for this test -
/// keeps the test from touching the real secure-storage plugin channel.
class _PendingAuthController extends AuthController {
  @override
  Future<UserProfile?> build() => Completer<UserProfile?>().future;
}

void main() {
  testWidgets('App shows the splash screen while auth state resolves', (tester) async {
    await tester.pumpWidget(
      ProviderScope(
        overrides: [authControllerProvider.overrideWith(_PendingAuthController.new)],
        child: const GrandmasDreamNumbersApp(),
      ),
    );

    expect(find.text("Grandma's Dream Numbers"), findsOneWidget);
  });
}
