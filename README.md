# AgentFramework - Branch "Introduce Azure"
Verwendung das Microsoft Agent Framework am Beispiel einer Azure Container App Anwendung.

Es wird darauf hingearbeitet, folgende Aufgabenstellung zu erfüllen:
 
Zur Zielerreichung plane ich die fiktive Anwendung ["TripAdvisor-Agent-App"](https://github.com/stinalina/CoCo-Frontend), Für jeden Kontinent soll es einen eigenen Agenten geben, welcher auf die speziellen Gegebenheiten und Kulturen spezialisiert ist. Nutzer der App können gezielt nach Reisezielen eines Landes oder Kontinents fragen, woraufhin der passende Agent ausgewählt wird. Sobald der Agent ausreichend Informationen vom Nutzer erhalten hat, wird der Prozess zur Reiseerstellung gestartet, welche auch wieder ein Agent übernimmt. Der gesamte Prozess von der Auswahl bis zur Buchung wird über einen Workflow orchestriert. Folgende Technologien und Frameworks sollen zum Einsatz kommen: 
  • Microsoft Agent Framework: zur Implementierung der Agenten und des Workflows
  • MCP Server für das spezifische bereitstellen der fiktiven Daten (hier Wikipedia)
  • Function Tools
  
# AgentFramework - Branch "Use local LLM with Ollama"
Der Code wurde als Console App angepasst, sodass nun Ollama genutzt werden kann. Ein Endpunkt wird somit nicht mehr gestellt.
