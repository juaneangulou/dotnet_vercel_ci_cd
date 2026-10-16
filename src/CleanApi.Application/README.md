# Application

Aqui viven los casos de uso, los puertos hacia recursos externos y DTO de salida.
`ProductService` coordina consultas/cotizaciones; `IProductRepository` define lo
que necesita sin conocer como se guardan los productos.

Agregar: casos de uso pequenos, contratos de repositorios/servicios, resultados
y coordinacion de operaciones. Propagar `CancellationToken` en operaciones I/O.
No agregar: SQL, DbContext, controladores, codigos HTTP, SDK de proveedores
ni lectura directa de configuracion del host.

Solo depende de Domain. Las invariantes siguen en Domain; los contratos de entrada
HTTP y su validacion estructural viven en Api. No se introduce MediatR/CQRS por
defecto: se pueden adoptar cuando la complejidad real los justifique.
