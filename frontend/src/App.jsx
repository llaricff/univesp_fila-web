import { useState } from 'react'
import CadastroForm from './components/CadastroForm.jsx'
import Confirmacao from './components/Confirmacao.jsx'
import { isMock } from './services/api.js'

const UNIDADE = 'UPA Sertãozinho'

const CHECKS = [
  'Cadastro em menos de 1 minuto',
  'Acompanhe sua posição em tempo real',
  'Casos de emergência mantêm prioridade',
]

function Brand() {
  return (
    <div className="brand">
      <div className="brand-mark" aria-hidden="true">
        <svg viewBox="0 0 24 24" fill="none" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
          <path d="M12 8v8M8 12h8" />
          <circle cx="12" cy="12" r="9" />
        </svg>
      </div>
      <span className="brand-name">Fila Web</span>
    </div>
  )
}

export default function App() {
  // Quando o usuário conclui o cadastro, guardamos a resposta do back-end aqui.
  const [resultado, setResultado] = useState(null)

  return (
    <div className="page">
      <header className="topbar">
        <Brand />
      </header>

      <main className="layout">
        <section className="hero">
          <span className="badge">
            <svg viewBox="0 0 24 24" fill="none" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
              <path d="M12 21s-7-6.1-7-11a7 7 0 0 1 14 0c0 4.9-7 11-7 11z" />
              <circle cx="12" cy="10" r="2.5" />
            </svg>
            {UNIDADE}
          </span>
          <h1>Entre na fila de triagem sem sair de casa</h1>
          <p className="lead">
            Cadastre-se pelo celular ou computador, reserve sua vez e acompanhe a posição da fila em
            tempo real. Menos tempo em pé, mais organização para todos.
          </p>
          <ul className="checks">
            {CHECKS.map((text) => (
              <li key={text}>
                <svg viewBox="0 0 24 24" fill="none" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
                  <path d="M20 6 9 17l-5-5" />
                </svg>
                {text}
              </li>
            ))}
          </ul>
        </section>

        <section className="card" aria-live="polite">
          {resultado ? (
            <Confirmacao resultado={resultado} unidade={UNIDADE} onNovo={() => setResultado(null)} />
          ) : (
            <CadastroForm onSucesso={setResultado} />
          )}
        </section>
      </main>

      {isMock && (
        <p className="mock-note">
          Modo de demonstração: nenhum dado é enviado a um servidor. Defina <code>VITE_API_URL</code> para usar o
          back-end.
        </p>
      )}
    </div>
  )
}
