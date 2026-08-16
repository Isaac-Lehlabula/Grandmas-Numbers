import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../core/storage/secure_storage_service.dart';
import '../data/auth_api.dart';
import '../data/auth_models.dart';

final authControllerProvider =
    AsyncNotifierProvider<AuthController, UserProfile?>(AuthController.new);

/// Holds the current session: null when signed out, the profile when
/// signed in. Screens watch this to know whether to show auth flows.
class AuthController extends AsyncNotifier<UserProfile?> {
  @override
  Future<UserProfile?> build() async {
    final secureStorage = ref.watch(secureStorageProvider);
    final accessToken = await secureStorage.readAccessToken();
    if (accessToken == null) {
      return null;
    }

    try {
      return await ref.read(authApiProvider).getProfile();
    } catch (_) {
      await secureStorage.clearTokens();
      return null;
    }
  }

  Future<void> register({
    required String firstName,
    required String email,
    required String password,
  }) async {
    state = const AsyncLoading();
    state = await AsyncValue.guard(() async {
      final session = await ref
          .read(authApiProvider)
          .register(firstName: firstName, email: email, password: password);
      await _persistSession(session);
      return session.user;
    });
  }

  Future<void> login({required String email, required String password}) async {
    state = const AsyncLoading();
    state = await AsyncValue.guard(() async {
      final session = await ref.read(authApiProvider).login(email: email, password: password);
      await _persistSession(session);
      return session.user;
    });
  }

  Future<void> logout() async {
    final secureStorage = ref.read(secureStorageProvider);
    final refreshToken = await secureStorage.readRefreshToken();

    if (refreshToken != null) {
      try {
        await ref.read(authApiProvider).logout(refreshToken);
      } catch (_) {
        // Best-effort server-side revoke - still clear the local session.
      }
    }

    await secureStorage.clearTokens();
    state = const AsyncData(null);
  }

  Future<void> _persistSession(AuthSession session) async {
    final secureStorage = ref.read(secureStorageProvider);
    await secureStorage.saveAccessToken(session.accessToken);
    await secureStorage.saveRefreshToken(session.refreshToken);
  }
}
