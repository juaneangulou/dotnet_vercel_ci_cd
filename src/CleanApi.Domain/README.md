# Domain

Aqui viven las entidades, value objects, invariantes y reglas del negocio.
`Product` impide identidades vacias, nombres vacios, precios negativos y
cantidades fuera del intervalo permitido; calcula importes con `decimal`.

Agregar: entidades como `Order`, estados validos y reglas independientes de HTTP.
No agregar: controladores, DTO HTTP, EF Core, SQL, configuracion, logs de
infraestructura, inyeccion de dependencias ni acceso a variables de entorno.

No referencia otros proyectos. Sus reglas se prueban sin servidor ni base de datos.
