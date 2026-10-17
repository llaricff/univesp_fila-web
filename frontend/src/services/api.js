// Camada de comunicação com o back-end (.NET).
//
// CONTRATO COMBINADO COM O BACK-END (ajustem juntos se mudar):
//
//   POST {VITE_API_URL}/api/fila
//   Corpo (JSON):
//     {
//       "nome": "Maria da Silva",
//       "dataNascimento": "1990-05-21",   // ISO yyyy-MM-dd
//       "cpf": "12345678909",             // só dígitos
//       "telefone": "16912345678",        // só dígitos
//       "motivo": "sintomas"              // "sintomas" | "lesao" | "outro"
//     }
//   Resposta 201 (JSON):
//     { "id": "abc123", "posicao": 7, "mensagem": "opcional" }
//   Erros: 400 com { "mensagem": "..." } e/ou { "erros": { "campo": "texto" } }
//
// Sem VITE_API_URL definido, o modo simulado responde sem precisar de back-end.

const API_URL = (import.meta.env.VITE_API_URL ?? '').replace(/\/$/, '')
const USE_MOCK = import.meta.env.VITE_API_URL === undefined || API_URL === ''

export const isMock = USE_MOCK

const wait = (ms) => new Promise((resolve) => setTimeout(resolve, ms))

export async function entrarNaFila(dados) {
  if (USE_MOCK) {
    await wait(800)
    return { id: crypto.randomUUID(), posicao: Math.floor(Math.random() * 12) + 1 }
  }

  let response
  try {
    response = await fetch(`${API_URL}/api/fila`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(dados),
    })
  } catch {
    throw new Error('Não foi possível conectar ao servidor. Tente novamente em instantes.')
  }

  const body = await response.json().catch(() => ({}))
  if (!response.ok) {
    const error = new Error(body.mensagem || 'Não foi possível concluir o cadastro.')
    error.fieldErrors = body.erros || {}
    throw error
  }
  return body
}
