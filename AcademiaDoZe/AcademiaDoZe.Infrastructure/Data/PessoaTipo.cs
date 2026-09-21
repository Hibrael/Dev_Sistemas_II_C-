//Hibrael Andre Cidade Xavier
namespace AcademiaDoZe.Infrastructure.Data
{
    /// <summary>
    /// Discriminador da coluna pessoa_tipo de tb_acesso.
    ///
    /// A tabela de acessos é única e polimórfica (id_acesso, pessoa_tipo, pessoa_id, data_hora):
    /// guarda o check-in tanto de aluno quanto de colaborador, e o par pessoa_tipo/pessoa_id diz
    /// para qual tabela o registro aponta. O domínio, por outro lado, tem duas entidades e dois
    /// repositórios distintos — então é aqui, na Infrastructure, que essa diferença é resolvida.
    ///
    /// Este é um detalhe de persistência: não vaza para o domínio nem para a aplicação. Os
    /// valores são fixos porque já existem linhas gravadas com eles; não os renumere.
    /// </summary>
    public enum PessoaTipo
    {
        Aluno = 1,
        Colaborador = 2
    }
}
