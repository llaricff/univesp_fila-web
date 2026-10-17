// Funções de máscara e validação dos campos do cadastro.

export const onlyDigits = (value) => value.replace(/\D/g, '')

export function maskCpf(value) {
  const d = onlyDigits(value).slice(0, 11)
  return d
    .replace(/^(\d{3})(\d)/, '$1.$2')
    .replace(/^(\d{3})\.(\d{3})(\d)/, '$1.$2.$3')
    .replace(/\.(\d{3})(\d)/, '.$1-$2')
}

export function maskPhone(value) {
  const d = onlyDigits(value).slice(0, 11)
  if (d.length <= 2) return d.length ? `(${d}` : ''
  if (d.length <= 6) return `(${d.slice(0, 2)}) ${d.slice(2)}`
  if (d.length <= 10) return `(${d.slice(0, 2)}) ${d.slice(2, 6)}-${d.slice(6)}`
  return `(${d.slice(0, 2)}) ${d.slice(2, 7)}-${d.slice(7)}`
}

export function maskDate(value) {
  const d = onlyDigits(value).slice(0, 8)
  return d.replace(/^(\d{2})(\d)/, '$1/$2').replace(/^(\d{2})\/(\d{2})(\d)/, '$1/$2/$3')
}

// Valida os dígitos verificadores do CPF.
export function isValidCpf(value) {
  const cpf = onlyDigits(value)
  if (cpf.length !== 11 || /^(\d)\1{10}$/.test(cpf)) return false
  const calc = (len) => {
    let sum = 0
    for (let i = 0; i < len; i++) sum += Number(cpf[i]) * (len + 1 - i)
    const r = (sum * 10) % 11
    return r === 10 ? 0 : r
  }
  return calc(9) === Number(cpf[9]) && calc(10) === Number(cpf[10])
}

// Aceita dd/mm/aaaa, data real, no passado e com idade até 120 anos.
export function parseBirthDate(value) {
  const m = /^(\d{2})\/(\d{2})\/(\d{4})$/.exec(value)
  if (!m) return null
  const [, dd, mm, yyyy] = m.map(Number)
  const date = new Date(yyyy, mm - 1, dd)
  const real = date.getFullYear() === yyyy && date.getMonth() === mm - 1 && date.getDate() === dd
  if (!real) return null
  const now = new Date()
  if (date > now) return null
  if (now.getFullYear() - yyyy > 120) return null
  return date
}

export function toIsoDate(date) {
  const p = (n) => String(n).padStart(2, '0')
  return `${date.getFullYear()}-${p(date.getMonth() + 1)}-${p(date.getDate())}`
}

export function isValidPhone(value) {
  const d = onlyDigits(value)
  return d.length === 10 || d.length === 11
}

export function validate(form) {
  const errors = {}
  if (form.nome.trim().split(/\s+/).filter(Boolean).length < 2) {
    errors.nome = 'Informe nome e sobrenome.'
  }
  if (!parseBirthDate(form.nascimento)) {
    errors.nascimento = 'Use uma data válida (dd/mm/aaaa).'
  }
  if (!isValidCpf(form.cpf)) {
    errors.cpf = 'CPF inválido.'
  }
  if (!isValidPhone(form.telefone)) {
    errors.telefone = 'Informe DDD e número.'
  }
  return errors
}
