import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../core/network/api_client.dart';
import 'dream_models.dart';

final dreamsApiProvider = Provider<DreamsApi>(
  (ref) => DreamsApi(ref.watch(apiClientProvider).dio),
);

class DreamsApi {
  DreamsApi(this._dio);

  final Dio _dio;

  Future<DreamAnalysisResult> analyse(String dreamText) async {
    final response = await _dio.post('/dreams/analyse', data: {'dreamText': dreamText});
    return DreamAnalysisResult.fromJson(response.data as Map<String, dynamic>);
  }

  Future<List<DreamSummary>> getHistory({String? query}) async {
    final response = await _dio.get(
      '/dreams',
      queryParameters: (query != null && query.isNotEmpty) ? {'query': query} : null,
    );
    return (response.data as List)
        .map((d) => DreamSummary.fromJson(d as Map<String, dynamic>))
        .toList();
  }

  Future<DreamAnalysisResult> getDetail(String id) async {
    final response = await _dio.get('/dreams/$id');
    return DreamAnalysisResult.fromJson(response.data as Map<String, dynamic>);
  }

  Future<void> delete(String id) => _dio.delete('/dreams/$id');

  Future<DreamAnalysisResult> analyseAgain(String id) async {
    final response = await _dio.post('/dreams/$id/analyse-again');
    return DreamAnalysisResult.fromJson(response.data as Map<String, dynamic>);
  }
}
