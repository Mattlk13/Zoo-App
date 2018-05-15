﻿app.factory('animalService', ['httpService', function (httpService) {
    var serviceBaseUrl = 'animals';

    var getPreservation = function () {
        return [
            { value: 7, format: 'לא ידוע' },
            { value: 1, format: 'ללא חשש' },
            { value: 2, format: 'קרוב לאיום' },
            { value: 3, format: 'פגיע' },
            { value: 4, format: 'בסכנת הכחדה' },
            { value: 5, format:'בסכנת הכחדה חמורה' },
            { value: 6, format: 'נכחד בטבע' }
        ];
    }

    var animalService = {
        getAllAnimals:                  function (language)             { return httpService.httpGet({ url: [serviceBaseUrl, 'all', language] }); },
        getAnimalById:                  function (animalId, language)   { return httpService.httpGet({ url: [serviceBaseUrl, 'id', animalId, language] }); },
        getAnimalsByEnclosure:          function (encId, language)      { return httpService.httpGet({ url: [serviceBaseUrl, 'enclosure', encId] }); },
        getAnimalDetailsById:           function (animalId)             { return httpService.httpGet({ url: [serviceBaseUrl, 'details', 'all', animalId]} ) },
        getAnimalStoriesByEnclosure:    function (encId, language)      { return httpService.httpGet({ url: [serviceBaseUrl, 'story', 'enclosure', encId] }); },
        getAnimalStoryDetailsById:      function (animalStoryId)        { return httpService.httpGet({ url: [serviceBaseUrl, 'story', 'details', 'all', animalStoryId]} ) },
        getAnimalByName:                function (name, language)       { return httpService.httpGet({ url: [serviceBaseUrl, 'name', name, language] }); },
        updateAnimal:                   function (animal)               { return httpService.httpPost({ url: [serviceBaseUrl, 'update'], body: animal }); },
        updateAnimalStory:              function (animalStory)          { return httpService.httpPost({ url: [serviceBaseUrl, 'story', 'update'], body: animalStory }); },
        updateAnimalDetail:             function (animalDetail)         { return httpService.httpPost({ url: [serviceBaseUrl, 'detail', 'update'], body: animalDetail }); },
        updateAnimalStoryDetail:        function (animalStoryDetail)    { return httpService.httpPost({ url: [serviceBaseUrl, 'story', 'detail', 'update'], body: animalStoryDetail }); },
        deleteAnimal:                   function (animalId)             { return httpService.httpDelete({ url: [serviceBaseUrl, 'delete', animalId] }); },
        getPreservation
    };

    return animalService;
}]);