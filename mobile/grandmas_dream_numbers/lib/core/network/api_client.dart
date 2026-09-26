import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../storage/secure_storage_service.dart';

/// Placeholder base URL. Point this at the local backend during development
/// (10.0.2.2 for the Android emulator's host loopback) and move it to build
/// configuration before release.
const String _baseUrl = 'http://localhost:5210/api';

final apiClientProvider = Provider<ApiClient>((ref) {
  return ApiClient(ref.watch(secureStorageProvider));
});

/// Thin wrapper around [Dio]: attaches the access token to every request,
/// and on a 401 tries once to refresh the session and retry before giving
/// up and clearing the stored tokens.
class ApiClient {
  ApiClient(this._secureStorage) {
    _dio = Dio(
      BaseOptions(
        baseUrl: _baseUrl,
        connectTimeout: const Duration(seconds: 10),
        receiveTimeout: const Duration(seconds: 10),
      ),
    );
    _refreshDio = Dio(BaseOptions(baseUrl: _baseUrl));

    _dio.interceptors.add(
      InterceptorsWrapper(
        onRequest: (options, handler) async {
          final token = await _secureStorage.readAccessToken();
          if (token != null) {
            options.headers['Authorization'] = 'Bearer $token';
          }
          handler.next(options);
        },
        onError: (error, handler) async {
          final isUnauthorized = error.response?.statusCode == 401;
          final isRetry = error.requestOptions.extra['retried'] == true;

          if (!isUnauthorized || isRetry) {
            handler.next(error);
            return;
          }

          final refreshToken = await _secureStorage.readRefreshToken();
          if (refreshToken == null) {
            handler.next(error);
            return;
          }

          try {
            final response = await _refreshDio.post(
              '/auth/refresh',
              data: {'refreshToken': refreshToken},
            );
            final newAccessToken = response.data['accessToken'] as String;
            final newRefreshToken = response.data['refreshToken'] as String;

            await _secureStorage.saveAccessToken(newAccessToken);
            await _secureStorage.saveRefreshToken(newRefreshToken);

            final retryOptions = error.requestOptions
              ..headers['Authorization'] = 'Bearer $newAccessToken'
              ..extra['retried'] = true;

            final retryResponse = await _dio.fetch(retryOptions);
            handler.resolve(retryResponse);
          } catch (_) {
            await _secureStorage.clearTokens();
            handler.next(error);
          }
        },
      ),
    );
  }

  final SecureStorageService _secureStorage;
  late final Dio _dio;
  late final Dio _refreshDio;

  Dio get dio => _dio;
}
