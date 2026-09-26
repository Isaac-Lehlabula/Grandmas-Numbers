import 'package:dio/dio.dart';

/// Pulls a human-readable message out of the backend's error shapes:
/// FluentValidation-style `{errors: {field: [msg]}}` from [ValidationFilter],
/// or the plain `{errors: [msg]}` list from auth failures.
String describeApiError(Object error) {
  if (error is! DioException) {
    return 'Something went wrong. Please try again.';
  }

  final data = error.response?.data;
  if (data is Map<String, dynamic>) {
    final errors = data['errors'];
    if (errors is List) {
      return errors.map((e) => e.toString()).join(' ');
    }
    if (errors is Map) {
      return errors.values
          .expand((v) => v is List ? v : [v])
          .map((e) => e.toString())
          .join(' ');
    }
    final detail = data['detail'];
    if (detail is String && detail.isNotEmpty) {
      return detail;
    }
  }

  if (error.type == DioExceptionType.connectionTimeout ||
      error.type == DioExceptionType.connectionError) {
    return "Couldn't reach Gogo's kitchen — check your connection and try again.";
  }

  return 'Something went wrong. Please try again.';
}
