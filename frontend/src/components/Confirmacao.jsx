export default function Confirmacao({ resultado, unidade, onNovo }) {
  return (
    <div className="confirm">
      <div className="confirm-icon" aria-hidden="true">
        <svg viewBox="0 0 24 24" fill="none" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
          <path d="M20 6 9 17l-5-5" />
        </svg>
      </div>
      <h2>Tudo certo, {resultado.nome}!</h2>
      <p className="lead">Você entrou na fila de triagem da {unidade}.</p>

      {resultado.posicao != null && (
        <div className="position">
          <span className="position-label">Sua posição na fila</span>
          <span className="position-number">{resultado.posicao}º</span>
        </div>
      )}

      <p className="footnote">
        Guarde seu código de acompanhamento: <strong className="code">{String(resultado.id).slice(0, 8)}</strong>.
        Avisaremos por telefone quando estiver perto da sua vez.
      </p>

      <button type="button" className="secondary" onClick={onNovo}>
        Fazer outro cadastro
      </button>
    </div>
  )
}
