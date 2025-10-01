const out = document.getElementById('out') as HTMLPreElement | null;
const btn = document.getElementById('btn') as HTMLButtonElement | null;

function getBackendUrl(): string {
  // @ts-ignore preload exposes api
  const fromBridge = (globalThis as any).api?.backendUrl as string | undefined;
  return fromBridge ?? 'http://localhost:5299';
}

async function ping() {
  const url = `${getBackendUrl()}/healthz`;
  try {
    out && (out.textContent = `GET ${url}\n...`);
    const r = await fetch(url);
    const json = await r.json();
    out && (out.textContent = `GET ${url}\n` + JSON.stringify(json, null, 2));
  } catch (e) {
    out && (out.textContent = `GET ${url}\nERROR: ${String(e)}`);
  }
}

btn?.addEventListener('click', () => { void ping(); });

// auto-ping on load
void ping();
