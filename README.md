# Agenda de la consultora — Fase 1: gestión de personas

Aplicación de consola en C# (.NET 8) para dar de alta, listar, buscar, modificar y dar de baja personas.

## Cómo ejecutarla

**Desde la terminal de VS Code**, en la carpeta `Proyecto1`:

```bash
dotnet run
```

Se recomienda `dotnet run` en la terminal integrada: si se lanza con F5, la configuración de depuración
debe usar `"console": "integratedTerminal"`, porque la consola de depuración de VS Code no permite escribir.

Requisitos: .NET SDK 10 (`dotnet --version`).

## Estructura

El código está organizado en capas. Cada una solo conoce a la de debajo:

```
Interfaz  →  Servicios  →  Datos
   (consola)   (reglas)      (almacenamiento)
        \          |          /
         └──── Modelos ──────┘
```

```
Proyecto1/
├── Proyecto1.csproj
├── Program.cs                        Crea las piezas, las conecta y arranca el menú
├── Modelos/
│   └── Persona.cs                    Datos de una persona
├── Datos/
│   ├── IRepositorioPersonas.cs       Contrato de almacenamiento
│   ├── RepositorioPersonasMemoria.cs Implementación con List<Persona>
│   └── DatosDeEjemplo.cs             4 personas para la demo
├── Servicios/
│   ├── ServicioPersonas.cs           Alta, búsqueda, modificación, baja, correo único
│   ├── ValidadorPersona.cs           Formato y normalización de cada dato
│   └── Resultado.cs                  Éxito o lista de errores de una operación
├── Interfaz/
│   ├── Menu.cs                       Menú numerado reutilizable (+ OpcionMenu)
│   ├── MenuPersonas.cs               Pantallas de alta, listado, búsqueda, modificación y baja
│   ├── CampoPersona.cs               Descripción de cada dato (etiqueta, validación, ancho…)
│   ├── Tabla.cs                      Dibuja cualquier lista como tabla alineada
│   └── Pantalla.cs                   Lectura validada y mensajes [OK] / [ERROR] / [AVISO]
└── Utilidades/
    ├── CodigoId.cs                   Id numérico ⇄ código P001
    └── Texto.cs                      Búsqueda sin mayúsculas ni tildes
```

## Decisiones de diseño

| Decisión | Justificación |
|---|---|
| **Capas separadas** (Modelos, Datos, Servicios, Interfaz) | Cada clase tiene una sola responsabilidad. Solo `Interfaz` usa `Console`, así que la lógica podrá reutilizarse cuando la aplicación tenga interfaz gráfica o web. |
| **Repositorio con interfaz** (`IRepositorioPersonas`) | Servicios y menús no saben dónde se guardan los datos. En la Fase 2 se añade `RepositorioPersonasBD` y solo cambia una línea de `Program.cs`. |
| **El repositorio en memoria trabaja con copias** | Se comporta como una base de datos: modificar un objeto no guarda nada hasta llamar a `Actualizar`. Así el código ya está escrito como funcionará con BD. |
| **Id `int` interno, código `P001` visible** | En una BD el Id será un entero autoincremental. `P001` es solo cómo se muestra y se busca. El contador solo avanza: un Id borrado nunca se reasigna, y no depende del nombre. |
| **Validación en el servicio** | Aunque la consola valida cada dato al escribirlo, el servicio vuelve a validar antes de guardar. Ninguna interfaz futura puede guardar datos incorrectos. |
| **`Resultado`** en vez de mensajes por pantalla | El servicio devuelve los errores y cada interfaz decide cómo mostrarlos. |
| **`CampoPersona`: cada dato definido una sola vez** | Alta, ficha, tabla y menú de modificar se generan a partir de la misma lista. Añadir un campo es añadir una línea en `MenuPersonas.CrearCampos()`. |
| **`Menu` y `Tabla` genéricos** | Sirven para el menú principal, el submenú de modificar y, en la Fase 2, para empresas. |
| Campo añadido: **Cargo** | En una agenda de consultora interesa saber el rol de cada contacto. Es opcional. |
| Empresa como texto libre opcional | En esta fase aún no existen empresas; en la Fase 2 pasará a `IdEmpresa`. |
| Menú sin limpiar pantalla | Toda la ejecución queda en la terminal, útil para las capturas de evidencia. |

## Validaciones

| Dato | Regla |
|---|---|
| Opción de menú | Si no es un número o no existe, muestra `[ERROR]` y repite el menú. El programa nunca se cierra por una entrada incorrecta. |
| Nombre y apellidos | Obligatorios y sin dígitos. |
| Teléfono | Obligatorio. Solo dígitos (opcionalmente con `+`), entre 9 y 15. Se admiten espacios y guiones al escribir; se guarda solo el número. |
| Correo | Obligatorio. Formato `usuario@dominio.ext`, sin espacios, no repetido. Se guarda en minúsculas. |
| Confirmaciones | Solo acepta S/SI/SÍ o N/NO. |

Si un dato es incorrecto se repite **solo ese campo**.

## Guion para la demostración (evidencia)

El programa arranca con 4 personas de ejemplo (P001–P004).

1. **Menú:** escribe `hola` y después `9` → error y el menú se repite.
2. **Listar (2):** tabla ordenada por apellidos.
3. **Alta (1):** prueba teléfono `abc` y correo `sin-arroba` para ver los errores; completa y confirma con `S` → `P005`.
4. **Buscar (3):** `garcia` (sin tilde) y después `p5`.
5. **Modificar (4):** primero `P099` → no existe. Vuelve a entrar y busca `ez` → varias coincidencias; elige `P002`, opción 3 (teléfono), confirma y termina con `0`.
6. **Baja (5):** elimina `P004` confirmando con `S`.
7. **Listar (2)** y **Salir (0)**.

## Qué cambiará en las siguientes fases

- **Fase 2 (empresas + base de datos):** añadir `Modelos/Empresa.cs`, `IRepositorioEmpresas`, `ServicioEmpresas`, `MenuEmpresas` y los repositorios de BD. `Persona.Empresa` (texto) pasa a `IdEmpresa`. `Menu`, `Tabla`, `Pantalla`, `CodigoId` (`E001`) y `Resultado` se reutilizan sin cambios.
- **Interfaz gráfica / web:** Modelos, Datos y Servicios no dependen de la consola; se podrán mover a una biblioteca de clases y reutilizar.

## Modelo de datos previsto (Fase 2 — provisional)

> Esquema orientativo. Se ajustará cuando se conozca el motor de base de datos; la sintaxis del
> autoincremental cambia según el motor (`AUTOINCREMENT` en SQLite, `IDENTITY` en SQL Server,
> `AUTO_INCREMENT` en MySQL).

### Relación

```
┌──────────────────────┐           ┌──────────────────────┐
│       EMPRESAS       │           │       PERSONAS       │
├──────────────────────┤           ├──────────────────────┤
│ PK IdEmpresa         │ 1       N │ PK IdPersona         │
│    NombreComercial   │───────────│ FK IdEmpresa (NULL)  │
│    Cif     (único)   │           │    Nombre            │
│    Telefono          │           │    Apellidos         │
│    Correo            │           │    Telefono          │
│    Direccion         │           │    Correo  (único)   │
└──────────────────────┘           │    Cargo             │
                                   └──────────────────────┘
```

- **Una empresa tiene varias personas (1:N)** y una persona pertenece como máximo a una empresa.
- `Personas.IdEmpresa` admite `NULL`: una persona puede no tener empresa asignada.
- Los Id son enteros autoincrementales generados por la base de datos; `P001` / `E001` es solo cómo se muestran.

### Tablas

| Tabla | Campo | Tipo | Restricciones |
|---|---|---|---|
| Empresas | IdEmpresa | entero | PK, autoincremental |
| | NombreComercial | texto(100) | obligatorio |
| | Cif | texto(9) | obligatorio, único |
| | Telefono | texto(16) | obligatorio |
| | Correo | texto(100) | obligatorio |
| | Direccion | texto(150) | obligatorio |
| Personas | IdPersona | entero | PK, autoincremental |
| | Nombre | texto(50) | obligatorio |
| | Apellidos | texto(100) | obligatorio |
| | Telefono | texto(16) | obligatorio |
| | Correo | texto(100) | obligatorio, único |
| | Cargo | texto(50) | opcional |
| | IdEmpresa | entero | FK → Empresas.IdEmpresa, admite NULL |

### Script orientativo

```sql
CREATE TABLE Empresas (
    IdEmpresa       INTEGER PRIMARY KEY,      -- autoincremental
    NombreComercial VARCHAR(100) NOT NULL,
    Cif             VARCHAR(9)   NOT NULL UNIQUE,
    Telefono        VARCHAR(16)  NOT NULL,
    Correo          VARCHAR(100) NOT NULL,
    Direccion       VARCHAR(150) NOT NULL
);

CREATE TABLE Personas (
    IdPersona  INTEGER PRIMARY KEY,           -- autoincremental
    Nombre     VARCHAR(50)  NOT NULL,
    Apellidos  VARCHAR(100) NOT NULL,
    Telefono   VARCHAR(16)  NOT NULL,
    Correo     VARCHAR(100) NOT NULL UNIQUE,
    Cargo      VARCHAR(50)  NULL,
    IdEmpresa  INTEGER      NULL,
    FOREIGN KEY (IdEmpresa) REFERENCES Empresas(IdEmpresa) ON DELETE SET NULL
);

-- Consulta final del proyecto: qué personas pertenecen a cada empresa
SELECT e.NombreComercial, p.Nombre, p.Apellidos, p.Cargo
FROM Empresas e
LEFT JOIN Personas p ON p.IdEmpresa = e.IdEmpresa
ORDER BY e.NombreComercial, p.Apellidos, p.Nombre;
```

### Decisiones pendientes

- **Borrar una empresa con personas:** se propone `ON DELETE SET NULL` (las personas se conservan sin empresa)
  y que la aplicación muestre antes las personas afectadas y pida confirmación. La alternativa es impedir el
  borrado mientras tenga personas (`ON DELETE RESTRICT`).

### Cambios en el código

| Ahora (Fase 1) | Fase 2 |
|---|---|
| `Persona.Empresa` (texto) | `Persona.IdEmpresa` (`int?`) |
| — | `Modelos/Empresa.cs` |
| `RepositorioPersonasMemoria` | `RepositorioPersonasBD` (implementa la misma `IRepositorioPersonas`) |
| — | `IRepositorioEmpresas` + `RepositorioEmpresasBD` |
| `Program.cs` l. 12 | Se cambia el repositorio en memoria por el de base de datos |
