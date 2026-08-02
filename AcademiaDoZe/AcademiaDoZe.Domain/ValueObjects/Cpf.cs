//Hibrael Andre Cidade Xavier
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace AcademiaDoZe.Domain.ValueObjects
{
    public class Cpf
    {
        public string Numero { get; private set; }

        public Cpf(string numero)
        {
            if (string.IsNullOrWhiteSpace(numero))
            {
                throw new ArgumentException("O CPF não pode ser vazio");
            }

            string textoLimpo = numero.Replace(".", "").Replace("-", "");

            if (!ValidarCpf(textoLimpo))
            {
                throw new ArgumentException("CPF inválido.");
            }

            Numero = textoLimpo;
        }

        private bool ValidarCpf(string cpf)
        {
            if (cpf is null || cpf.Length != 11)
            {
                return false;
            }

            if (new string(cpf[0], 11) == cpf)
            {
                return false;
            }

            int soma = SomaCpf(cpf, 9, 10);
            int digitoVerificador = GerarDigito(soma);
            bool primeiroDigitoValido = ValidarDigito(cpf, 9, digitoVerificador);
            if (!primeiroDigitoValido)
            {
                return false;
            }
            soma = SomaCpf(cpf, 10, 11);
            digitoVerificador = GerarDigito(soma);
            bool segundoDigitoValido = ValidarDigito(cpf, 10, digitoVerificador);
            
            if (!segundoDigitoValido)
            {
                return false;
            }
            return true;
        }

        private int GerarDigito(int soma)
        {
        int digitoVerificador = soma % 11;
        if (digitoVerificador < 2)
        {
            return 0;

        }
        else
        {
            return 11 - digitoVerificador;
        }

       
        }

        private int SomaCpf(string cpf, int tamanho, int peso)
        {
            int soma = 0;
            for (int i =0; i<tamanho; i++)
            {
                int numeroAtual = int.Parse(cpf[i].ToString());
                soma += numeroAtual * peso;
                peso--;
            }
            return soma;
        }

        private bool ValidarDigito(string cpf, int posicao, int digitoVerificador)
        {
            int digito = int.Parse(cpf[posicao].ToString());
            return digitoVerificador == digito;
        }


    }
}
    