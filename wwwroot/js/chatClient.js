const controllers = new Map();
let nextId = 1;

export async function streamChat(url, payload, dotNetRef) {
    const id = nextId++;
    const controller = new AbortController();
    controllers.set(id, controller);

    try {
        const response = await fetch(url, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "Accept": "text/event-stream"
            },
            body: JSON.stringify(payload),
            signal: controller.signal
        });

        if (!response.ok || !response.body) {
            await dotNetRef.invokeMethodAsync("OnStreamError", `HTTP ${response.status}`);
            controllers.delete(id);
            return id;
        }

        const reader = response.body.getReader();
        const decoder = new TextDecoder();
        let buffer = "";

        while (true) {
            const { done, value } = await reader.read();
            if (done) break;
            buffer += decoder.decode(value, { stream: true });

            let idx;
            while ((idx = buffer.indexOf("\n\n")) >= 0) {
                const raw = buffer.slice(0, idx).trim();
                buffer = buffer.slice(idx + 2);
                if (!raw.startsWith("data:")) continue;
                const data = raw.slice(5).trim();
                await dotNetRef.invokeMethodAsync("OnStreamChunk", data);
            }
        }

        await dotNetRef.invokeMethodAsync("OnStreamCompleted");
    } catch (err) {
        if (err?.name === "AbortError") {
            await dotNetRef.invokeMethodAsync("OnStreamError", "aborted");
        } else {
            await dotNetRef.invokeMethodAsync("OnStreamError", err?.message ?? "stream error");
        }
    } finally {
        controllers.delete(id);
    }

    return id;
}

export function cancelStream(id) {
    const controller = controllers.get(id);
    if (controller) {
        controller.abort();
        controllers.delete(id);
    }
}

export function scrollToBottom(element) {
    if (!element) return;
    requestAnimationFrame(() => {
        element.scrollTop = element.scrollHeight;
    });
}

export function getTimezoneOffset() {
    // Minutes between local time and UTC; JavaScript returns local - UTC.
    return new Date().getTimezoneOffset();
}
