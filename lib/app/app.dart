import 'package:flutter/material.dart';

import 'theme.dart';

class App extends StatelessWidget {
  const App({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'ACHS · Prototipo Directivos',
      debugShowCheckedModeBanner: false,
      theme: AppTheme.light(),
      darkTheme: AppTheme.dark(),
      // Temporary placeholder home. The next branch wires go_router and the
      // real auth + shell screens.
      home: const _ScaffoldPlaceholder(),
    );
  }
}

class _ScaffoldPlaceholder extends StatelessWidget {
  const _ScaffoldPlaceholder();

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return Scaffold(
      appBar: AppBar(
        title: const Text('ACHS · Prototipo Directivos'),
      ),
      body: Center(
        child: ConstrainedBox(
          constraints: const BoxConstraints(maxWidth: 480),
          child: Padding(
            padding: const EdgeInsets.all(AppTokens.spacing * 1.5),
            child: Column(
              mainAxisSize: MainAxisSize.min,
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  'App para Directivos',
                  style: theme.textTheme.headlineMedium,
                ),
                const SizedBox(height: AppTokens.spacing / 2),
                Text(
                  'Andamiaje inicial · capa de datos mock lista.',
                  style: theme.textTheme.bodyLarge,
                ),
                const SizedBox(height: AppTokens.spacing),
                Text(
                  'Las pantallas reales (login, shell, módulos) se construyen '
                  'en la siguiente rama.',
                  style: theme.textTheme.bodyMedium?.copyWith(
                    color: theme.colorScheme.onSurfaceVariant,
                  ),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}
