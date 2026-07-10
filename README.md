<a id="readme-top"></a>

[![Contributors][contributors-shield]][contributors-url]
[![Forks][forks-shield]][forks-url]
[![Stargazers][stars-shield]][stars-url]
[![Issues][issues-shield]][issues-url]
[![Unlicense License][license-shield]][license-url]
[![LinkedIn][linkedin-shield]][linkedin-url]



<!-- PROJECT LOGO -->
<br />
<div align="center">
  <h3 align="center">AI-Interviewer</h3>

  <p align="center">
    Give Unlimited Offline Technical as well as Non-Technical Interviews.
    <br />
    <a href="https://github.com/DJ2211/AI-Interviewer"><strong>Explore the docs »</strong></a>
    <br />
    <br />
    <a href="https://github.com/DJ2211/AI-Interviewer">View Demo</a>
    &middot;
    <a href="https://github.com/DJ2211/AI-Interviewer/issues/new?labels=bug&template=bug-report---.md">Report Bug</a>
    &middot;
    <a href="https://github.com/DJ2211/AI-Interviewer/issues/new?labels=enhancement&template=feature-request---.md">Request Feature</a>
  </p>
</div>



<!-- TABLE OF CONTENTS -->
<details>
  <summary>Table of Contents</summary>
  <ol>
    <li>
      <a href="#about-the-project">About The Project</a>
      <ul>
        <li><a href="#built-with">Built With</a></li>
      </ul>
    </li>
    <li>
      <a href="#getting-started">Getting Started</a>
      <ul>
        <li><a href="#Flow">Flow</a></li>
        <li><a href="#prerequisites">Prerequisites</a></li>
        <li><a href="#installation">Installation</a></li>
      </ul>
    </li>
    <li><a href="#usage">Usage</a></li>
    <li><a href="#roadmap">Roadmap</a></li>
    <li><a href="#contributing">Contributing</a></li>
    <li><a href="#license">License</a></li>
    <li><a href="#contact">Contact</a></li>
    <li><a href="#acknowledgments">Acknowledgments</a></li>
  </ol>
</details>



<!-- ABOUT THE PROJECT -->
## About The Project
Practice Real-Time Unlimited Technical/Non-Technical Interviews locally and also get transcription of entire interview for verdict.



### Built With

* [![.NET][Dotnet]][Dotnet-url]
* [![Ollama][Ollama]][Ollama-url]



## Getting Started

Follow below steps to set up the AI Interviewer locally. The application relies on a .NET backend, a lightweight HTML/JS frontend, Ollama for the conversational LLM, and Kokoro for local Text-to-Speech processing.



### Flow
* You Start the interview by filling the Form From the UI and click Start Interview button
*  Make sure ollama model is running on port http://localhost:11434
* KokoroTTS is installed along with its voice command

* loop this below two steps for certain number of questions
  * Interviewer will introduce herself, and ask questions
  * You give answers according to your best knowledge

* transcription will be generated copy it and paste it into your favorite LLM (Chatgpt/DeepSeek/Gemini/Claude, etc) 

* Hurray you will get your verdict along with all the key matrics you have to take care of.

* If you want to change the style of interview or LocalLLM Model (Remember it is the brain of your Interviewer so the powerfull the model the good the interview),
   but we have to take care of bigger models, as the models will be appended in GPU, and if GPU is not able to fit it, then it will get appended in CPU, if memory is also not able to fit it. then it will give 
   very unpleasent errors, and as a developer we dont want that :)

* Enjoy the interview, repeat till get better. 



### Prerequisites

You will need the following software installed on your system:

* **Windows 10/11** (temporary as Testing for Mac and Linux is remaining)
  
* **.NET 10 SDK** (For running Backend Project)
  [Download .NET 10 SDK](https://builds.dotnet.microsoft.com/dotnet/Sdk/10.0.301/dotnet-sdk-10.0.301-win-x64.exe)
  
* **Ollama** (For running the local LLM)
  [Download Ollama](https://ollama.com/download)
  
* **KokoroTTS** (For running TTS Service)
* [Download_KokoroTTS]()

**Required Local AI Models:**
Before running the application, pull the required open-source LLM via your terminal using Ollama:
It consist of 4.9 GB and it will work as your own Local LLM like chatgpt, claude, etc
```sh
ollama run llama3.1:8b-instruct-q4_K_M
```

Now We need to install KokoroTTS model:
Its responsible of hearing what LLM says to the output
```sh
remaining
```

  

### Installation

1. Clone the repo
```sh
git clone https://github.com/DJ2211/AI-Interviewer.git
cd AI-Interviewer
```

2. Add the Kokoro TTS Model
Because of GitHub's file size limits, the Text-to-Speech model is not included in the repository.
  
Download the kokoro.onnx model from [].
Place the kokoro.onnx file directly into your API folder (or wherever your code is expecting to find it).

3. Start the LLM
Open Ollama and run your downloaded LLM from the terminal:

```sh
ollama run llama3.1:8b-instruct-q4_K_M
```

4. Configure the API
Go to the API Project and update the appsettings.json file to match your Ollama setup. Ensure the BaseUrl and ModelName match exactly.

```JSON
"LLM": {
  "BaseUrl": "http://localhost:11434",
  "ModelName": "llama3.1:8b-instruct-q4_K_M",
  "Temperature": 0.7,
  "MaxTokens": 1024
}
```

5. Install the Kokoro TTS onnx model

6. Run the Backend API
Open a terminal in your API project folder and start the server:
  
```sh
cd API
dotnet run
```
(Ensure it is running on http://localhost:5075, as expected by your frontend).

7. Open the UI [UI](https://github.com/DJ2211/AI-Interviewer-UI)
Since frontend is a plain HTML file, simply open index.html in your web browser (or use an extension like VS Code Live Server) to start your interview.



<!-- USAGE EXAMPLES -->
## Usage

If You want to practice Real time Interview to ace Interviews, or to remove Fear of Giving interviews in Pressure, this is the Ultimate project which will help you.
_For more examples, please refer to the [Documentation](https://example.com)_



<!-- ROADMAP -->
## Roadmap

- [x] Services for Text To Speech(TTS), Speech To Text(STT), Local Language Model (LLM) As a Interviewer Brain
- [x] API Project to run Smoothly
- [ ] Feature to also Take DSA Interview using OpenSource Code Editor

See the [open issues](https://github.com/DJ2211/AI-Interviewer/issues) for a full list of proposed features (and known issues).

<p align="right">(<a href="#readme-top">back to top</a>)</p>



<!-- CONTRIBUTING -->
## Contributing

Contributions are what make the open source community such an amazing place to learn, inspire, and create. Any contributions you make are **greatly appreciated**.

If you have a suggestion that would make this better, please fork the repo and create a pull request. You can also simply open an issue with the tag "enhancement".
Don't forget to give the project a star! Thanks again!

1. Fork the Project
2. Create your Feature Branch (`git checkout -b feature/AmazingFeature`)
3. Commit your Changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the Branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

### Top contributors:

<a href="https://github.com/DJ2211/AI-Interviewer/graphs/contributors">
  <img src="https://contrib.rocks/image?repo=DJ2211/AI-Interviewer" />
</a>

Made with [contrib.rocks](https://contrib.rocks).



<!-- LICENSE -->
## License

Distributed under the MIT License. See `LICENSE.txt` for more information.

<p align="right">(<a href="#readme-top">back to top</a>)</p>



<!-- CONTACT -->
## Contact

Your Name - [@linkedin](https://www.linkedin.com/in/jay-dholakia-a63103187/)

Project Link: [https://github.com](https://github.com/DJ2211/AI-Interviewer)

<p align="right">(<a href="#readme-top">back to top</a>)</p>



<!-- ACKNOWLEDGMENTS -->
## Acknowledgments



<!-- MARKDOWN LINKS & IMAGES -->
<!-- https://www.markdownguide.org/basic-syntax/#reference-style-links -->
[contributors-shield]: https://img.shields.io/github/contributors/DJ2211/AI-Interviewer.svg?style=for-the-badge
[contributors-url]: https://github.com/DJ2211/AI-Interviewer/graphs/contributors
[forks-shield]: https://img.shields.io/github/forks/DJ2211/AI-Interviewer.svg?style=for-the-badge
[forks-url]: https://github.com/DJ2211/AI-Interviewer/network/members
[stars-shield]: https://img.shields.io/github/stars/DJ2211/AI-Interviewer.svg?style=for-the-badge
[stars-url]: https://github.com/DJ2211/AI-Interviewer/stargazers
[issues-shield]: https://img.shields.io/github/issues/DJ2211/AI-Interviewer.svg?style=for-the-badge
[issues-url]: https://github.com/DJ2211/AI-Interviewer/issues
[license-shield]: https://img.shields.io/github/license/DJ2211/AI-Interviewer.svg?style=for-the-badge
[license-url]: https://github.com/DJ2211/AI-Interviewer/blob/master/LICENSE.txt
[linkedin-shield]: https://img.shields.io/badge/-LinkedIn-black.svg?style=for-the-badge&logo=linkedin&colorB=555
[linkedin-url]: https://www.linkedin.com/in/jay-dholakia-a63103187/
[product-screenshot]: images/project.gif
[Dotnet]: https://img.shields.io/badge/dotnet-512BD4?style=for-the-badge&logo=dotnet&logoColor=white
[Dotnet-url]: https://dotnet.microsoft.com/en-us/
[Ollama]: https://img.shields.io/badge/React-20232A?style=for-the-badge&logo=react&logoColor=61DAFB
[Ollama-url]: https://ollama.com/
