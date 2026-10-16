# Infrastructure

Aqui se implementan los puertos definidos por Application. `DemoProductRepository`
expone un catalogo fijo de solo lectura; no representa persistencia de produccion.
Es seguro como singleton porque no contiene estado mutable por peticion.

Agregar: repositorios reales, EF Core y migraciones, clientes HTTP, correo,
almacenamiento externo y adaptadores de terceros. Validar configuracion al inicio.
No agregar: reglas de negocio, controladores ni contratos que obliguen a
Application a depender de SDK externos.

Depende de Application (y transitivamente de Domain), nunca de Api. Al introducir
DbContext usa lifetime scoped, operaciones async y una base externa. No ejecutes
migraciones automaticamente en cada cold start ni guardes datos en disco local
en Vercel. Las credenciales vienen de configuracion segura del host.
