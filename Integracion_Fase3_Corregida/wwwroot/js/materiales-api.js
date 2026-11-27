// Cliente aislado para la futura API de materiales.
// Completar rutas y autenticación cuando el equipo publique el contrato.
// PENDIENTE DE INTEGRACIÓN:
// 1. Reemplazar las rutas null por las rutas reales de la API.
// 2. Confirmar el nombre de cada campo del JSON de respuesta y solicitud.
// 3. Definir autenticación: cookie o token JWT.
// 4. Si se usan cookies, confirmar cómo enviar el token antifalsificación CSRF.
// 5. Confirmar los códigos HTTP de éxito y error.

// Cliente Fetch para la API de materiales.
// Contrato basado en MaterialesApiController de Samuel.

const MaterialesApi = (() => {
    const base = "/api/materiales";

    const rutas = {
        listar: base,
        obtener: id => `${base}/${id}`,
        crear: base,
        actualizar: id => `${base}/${id}`,
        eliminar: id => `${base}/${id}`
    };

    async function solicitar(url, opciones = {}) {
        if (!url) {
            throw new Error("La ruta de la API no está configurada.");
        }

        let respuesta;

        try {
            respuesta = await fetch(url, {
                credentials: "same-origin",
                ...opciones,
                headers: {
                    Accept: "application/json",
                    ...(opciones.body
                        ? { "Content-Type": "application/json" }
                        : {}),
                    ...opciones.headers
                }
            });
        } catch {
            throw new Error("No se pudo conectar con el servidor.");
        }

        if (!respuesta.ok) {
            let detalle = `Error HTTP ${respuesta.status}.`;

            try {
                const tipo = respuesta.headers.get("content-type") || "";

                if (tipo.includes("application/json")) {
                    const errorJson = await respuesta.json();
                    detalle =
                        errorJson.mensaje ||
                        errorJson.detail ||
                        errorJson.title ||
                        errorJson.message ||
                        detalle;
                } else {
                    const texto = await respuesta.text();
                    if (texto) detalle = texto;
                }
            } catch {
                // Conserva el mensaje HTTP si la respuesta no se puede leer.
            }

            throw new Error(detalle);
        }

        if (respuesta.status === 204) {
            return null;
        }

        const tipo = respuesta.headers.get("content-type") || "";

        if (!tipo.includes("application/json")) {
            return null;
        }

        try {
            return await respuesta.json();
        } catch {
            throw new Error("El servidor devolvió una respuesta JSON inválida.");
        }
    }

    return {
        listar() {
            return solicitar(rutas.listar);
        },

        obtener(id) {
            return solicitar(rutas.obtener(id));
        },

        crear(material) {
            return solicitar(rutas.crear, {
                method: "POST",
                body: JSON.stringify(material)
            });
        },

        actualizar(id, material) {
            return solicitar(rutas.actualizar(id), {
                method: "PUT",
                body: JSON.stringify(material)
            });
        },

        eliminar(id) {
            return solicitar(rutas.eliminar(id), {
                method: "DELETE"
            });
        }
    };
})();