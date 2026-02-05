# Laboratorio - Seguridad de peticiones

**Objetivo**
Este laboratorio protege el acceso a endpoints del backend haciendo que no baste con un solo token. Se requiere un conjunto de datos (cookies + header) para que una peticion sea valida.

**Medidas aplicadas**
- El JWT se guarda en una cookie HttpOnly (no accesible desde JavaScript).
- El backend lee el JWT desde la cookie (no desde el header Authorization).
- Se usa cookie de sesion HttpOnly adicional (`lab_session`).
- Se usa token XSRF con double-submit: cookie `XSRF-TOKEN` + header `X-XSRF-TOKEN`.
- Cookies con `SameSite=Strict` y `Secure` en produccion.
- El login no devuelve el JWT en el body (reduce exposicion en JS).
- Se exige **JWT + sesion + XSRF** para responder endpoints protegidos.

**Como dificulta la simulacion de peticiones**
Una peticion valida necesita al mismo tiempo:
- Cookie HttpOnly con JWT (`lab_jwt`).
- Cookie HttpOnly de sesion (`lab_session`).
- Header `X-XSRF-TOKEN` que debe coincidir con la cookie `XSRF-TOKEN`.

Si falta alguno, el backend responde 401. Esto hace mas dificil copiar una peticion desde la red sin tener todas las piezas correctas.

**Endpoints relevantes**
- `POST /auth/login`: recibe `{ "apiKey": "<string encriptado>" }`, valida y crea cookies.
- `GET /api/getDocuments`: retorna tipos de documento, requiere autenticacion.

**Nota importante**
En aplicaciones web, cualquier secreto embebido en el frontend puede ser extraido por un usuario con acceso al navegador. Por eso se mueve el JWT a HttpOnly y se agrega XSRF, pero si necesitas seguridad total, el enfoque recomendado es un BFF (Backend for Frontend) o autenticacion real de usuarios.
