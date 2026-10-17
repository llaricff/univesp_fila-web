import { useState } from 'react'
import { entrarNaFila } from '../services/api.js'
import {
  maskCpf,
  maskDate,
  maskPhone,
  onlyDigits,
  parseBirthDate,
  toIsoDate,
  validate,
} from '../utils/validators.js'

const MOTIVOS = [
  { value: 'sintomas', label: 'Sintomas' },
  { value: 'lesao', label: 'Lesão / dor' },
  { value: 'outro', label: 'Outro' },
]

const INITIAL = { nome: '', nascimento: '', cpf: '', telefone: '', motivo: 'sintomas' }

function Field({ id, label, hint, error, children }) {
  return (
    <div className="field">
      <label htmlFor={id}>
        {label} {hint && <span className="hint">{hint}</span>}
      </label>
      {children}
      {error && (
        <p className="error" id={`${id}-erro`} role="alert">
          {error}
        </p>
      )}
    </div>
  )
}

export default function CadastroForm({ onSucesso }) {
  const [form, setForm] = useState(INITIAL)
  const [errors, setErrors] = useState({})
  const [enviando, setEnviando] = useState(false)
  const [erroGeral, setErroGeral] = useState('')

  const set = (name, value) => {
    setForm((f) => ({ ...f, [name]: value }))
    if (errors[name]) setErrors((e) => ({ ...e, [name]: undefined }))
  }

  const inputProps = (name, mask) => ({
    id: name,
    name,
    value: form[name],
    onChange: (e) => set(name, mask ? mask(e.target.value) : e.target.value),
    'aria-invalid': errors[name] ? 'true' : 'false',
    'aria-describedby': errors[name] ? `${name}-erro` : undefined,
    disabled: enviando,
  })

  async function handleSubmit(e) {
    e.preventDefault()
    setErroGeral('')
    const found = validate(form)
    setErrors(found)
    if (Object.keys(found).length > 0) return

    setEnviando(true)
    try {
      const resposta = await entrarNaFila({
        nome: form.nome.trim(),
        dataNascimento: toIsoDate(parseBirthDate(form.nascimento)),
        cpf: onlyDigits(form.cpf),
        telefone: onlyDigits(form.telefone),
        motivo: form.motivo,
      })
      onSucesso({ ...resposta, nome: form.nome.trim().split(/\s+/)[0] })
    } catch (err) {
      setErroGeral(err.message)
      if (err.fieldErrors) setErrors((prev) => ({ ...prev, ...err.fieldErrors }))
    } finally {
      setEnviando(false)
    }
  }

  return (
    <form onSubmit={handleSubmit} noValidate>
      <h2>Fazer meu cadastro</h2>

      <Field id="nome" label="Nome completo" error={errors.nome}>
        <input {...inputProps('nome')} type="text" autoComplete="name" placeholder="Como está no seu documento" />
      </Field>

      <div className="row">
        <Field id="nascimento" label="Nascimento" error={errors.nascimento}>
          <input
            {...inputProps('nascimento', maskDate)}
            type="text"
            inputMode="numeric"
            autoComplete="bday"
            placeholder="dd/mm/aaaa"
          />
        </Field>
        <Field id="cpf" label="CPF" error={errors.cpf}>
          <input {...inputProps('cpf', maskCpf)} type="text" inputMode="numeric" placeholder="000.000.000-00" />
        </Field>
      </div>

      <Field id="telefone" label="Telefone" hint="para avisar quando estiver perto da sua vez" error={errors.telefone}>
        <input
          {...inputProps('telefone', maskPhone)}
          type="tel"
          autoComplete="tel"
          inputMode="numeric"
          placeholder="(16) 90000-0000"
        />
      </Field>

      <fieldset className="field motivos" disabled={enviando}>
        <legend>Motivo da visita</legend>
        <div className="pills">
          {MOTIVOS.map((m) => (
            <label key={m.value} className={form.motivo === m.value ? 'pill selected' : 'pill'}>
              <input
                type="radio"
                name="motivo"
                value={m.value}
                checked={form.motivo === m.value}
                onChange={() => set('motivo', m.value)}
              />
              {m.label}
            </label>
          ))}
        </div>
      </fieldset>

      {erroGeral && (
        <p className="error general" role="alert">
          {erroGeral}
        </p>
      )}

      <button type="submit" className="primary" disabled={enviando}>
        {enviando ? 'Enviando…' : 'Entrar na fila'}
      </button>

      <p className="footnote">
        Casos de emergência têm prioridade e podem ser chamados fora da ordem de chegada.
      </p>
    </form>
  )
}
