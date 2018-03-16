TO DO:
	- garantir que currentSpot (Indexer) <x> e <y> sejam sempre validos
	
	- criar uma estrutura que contenha:
		* tamanho da estrada a ser contruida
		* numero de intersecções na estrada a ser construida
		* índiece das intersecções da estrada
		
	- criar uma função que receba a estrutura mencionada e contrua uma
	estrada a partir dela
	
	- revisar as funções de construção de estradas para garantir que
	nenhuma delas leve à um caminho irreversível
		* se houver um ploco de rua no alcance_maximo da construção
		* faça uma estrada até o (alcance_maximo - 1) e entao
		* no alcance_maximo crie uma curva
		
	- fazer com que o circuito seja fechado (concluido)