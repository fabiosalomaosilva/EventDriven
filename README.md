## Event Driven

Este repositório demonstra a implementação de uma arquitetura orientada a eventos usando .Net 8 e RabbitMQ. O projeto visa ilustrar como os serviços de pedidos e faturamento podem se comunicar de forma assíncrona através de eventos. Utilizando padrões de design como Observer e Publish/Subscribe, o sistema é construído para ser escalável, flexível e altamente reativo, especialmente adequado para cenários de microserviços.

### Objetivos do Projeto

- Demonstrar os conceitos fundamentais da arquitetura Event Driven Domain.
- Implementar um sistema de exemplo com serviços que comunicam-se via RabbitMQ.
- Utilizar .Net 8 para desenvolver serviços de pedidos e faturamento.
- Mostrar os benefícios do desacoplamento, escalabilidade e flexibilidade em uma arquitetura orientada a eventos.

### Tecnologias Utilizadas

- **.Net 8**: Framework para construção dos serviços.
- **RabbitMQ**: Sistema de mensageria para comunicação entre os serviços.
- **Docker**: Para facilitar a instalação e execução do RabbitMQ.
- **Visual Studio 2022**: Ambiente de desenvolvimento integrado.

### Estrutura do Projeto

O projeto está organizado em dois serviços principais:

1. **OrderService**: Responsável pela criação de pedidos e publicação de eventos no RabbitMQ.
2. **BillingService**: Consome os eventos de pedidos do RabbitMQ e gera as faturas correspondentes.

Cada serviço possui sua própria implementação e configuração, seguindo os princípios da arquitetura orientada a eventos.

### Como Contribuir

Contribuições são bem-vindas! Sinta-se à vontade para forkear este repositório e submeter um pull request com melhorias, correções ou novas funcionalidades.

### Licença

Este projeto está licenciado sob a licença MIT - veja o arquivo [LICENSE](LICENSE) para mais detalhes.
