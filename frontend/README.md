# Fila Web – Front-end

Tela de cadastro na fila de triagem da UPA Sertãozinho. React 19 + Vite, em JavaScript.

## Como rodar

Requisito: Node.js 20 ou superior.

```bash
cd frontend
npm install
npm run dev
```

Abra http://localhost:5173. Sem back-end configurado, a tela funciona em **modo de demonstração**
(o envio é simulado e nenhum dado sai do navegador).

## Ligando ao back-end (.NET)

1. Copie `.env.example` para `.env`.
2. Em desenvolvimento, aponte `VITE_PROXY_TARGET` para a porta em que o back-end está rodando
   (por exemplo `http://localhost:5000`) e defina `VITE_API_URL=http://localhost:5173`
   (o Vite encaminha `/api` para o back-end, o que evita configurar CORS no começo).
3. Em produção, defina `VITE_API_URL` com o endereço público da API e libere CORS para o domínio do front.

### Contrato da API (combinar com o back-end)

`POST /api/fila`

```json
{
  "nome": "Maria da Silva",
  "dataNascimento": "1990-05-21",
  "cpf": "12345678909",
  "telefone": "16912345678",
  "motivo": "sintomas"
}
```

- `motivo`: `sintomas`, `lesao` ou `outro`. CPF e telefone chegam só com dígitos.
- Resposta de sucesso (`201`): `{ "id": "abc123", "posicao": 7 }`
- Erro (`400`): `{ "mensagem": "texto geral", "erros": { "cpf": "CPF já está na fila" } }`.
  O front mostra `mensagem` no topo do botão e cada item de `erros` embaixo do campo correspondente
  (nomes de campo: `nome`, `nascimento`, `cpf`, `telefone`).

O código que chama a API está isolado em `src/services/api.js`.

## Estrutura

```
src/
  App.jsx                      página (texto de apresentação + cartão)
  components/CadastroForm.jsx  formulário com máscaras e validação
  components/Confirmacao.jsx   tela exibida após entrar na fila
  services/api.js              chamada ao back-end (ou simulação)
  utils/validators.js          máscaras e validação (CPF, data, telefone)
  styles.css                   estilos (responsivo: celular e desktop)
```

## Pendências conhecidas

- "Acompanhar minha posição": tela ainda não criada.
- Avisos por telefone citados na tela de confirmação dependem do back-end.
- Uso de CPF e dados de saúde exige atenção à LGPD antes de ir para produção real.
