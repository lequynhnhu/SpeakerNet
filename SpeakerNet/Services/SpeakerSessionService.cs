﻿using System;
using System.Collections.Generic;
using System.Linq;
using SpeakerNet.Data;
using SpeakerNet.Extensions;
using SpeakerNet.Models;
using SpeakerNet.ViewModels;

namespace SpeakerNet.Services
{
    public class 
        SpeakerSessionService : ISpeakerSessionService
    {
        private readonly IRepository<Session> _repository;
        private readonly IRepository<Speaker> _speakerRepository;
        private readonly IRepository<Event> _eventRepository;

        public SpeakerSessionService(IRepository<Session> repository, IRepository<Speaker> speakerRepository,
                                     IRepository<Event> eventRepository)
        {
            _repository = repository;
            _speakerRepository = speakerRepository;
            _eventRepository = eventRepository;
        }

        public SpeakerSessionListModel GetSpeakerSessionList(Guid speakerId)
        {
            var speaker = GetSpeaker(speakerId);
            var sessions = _repository.Entities.Where(s => s.Speaker.Id == speaker.Id).ToList();
            var model = speaker.MapFrom<Speaker, SpeakerSessionListModel>();
            model.Sessions = sessions.MapFrom<Session, SpeakerSessionIndexModel>();
            return model;
        }

        private Speaker GetSpeaker(Guid speakerId)
        {
            return _speakerRepository.Entities
                .Single(s => s.Id == speakerId);
        }

        private Event GetEvent(int eventId)
        {
            return _eventRepository.Entities.Single(e => e.Id == eventId);
        }

        public void CreateSession(Guid speakerId, CreateSessionModel model)
        {
            var speaker = GetSpeaker(speakerId);
            var theEvent = GetEvent(model.EventId);
            var session = Session.Create(model.Name, model.Abstract, model.Level, model.Duration);
            session.Speaker = speaker;
            session.Event = theEvent;
            _repository.Add(session);
            _repository.SaveChanges();
        }

        public IEnumerable<DetailsEventModel> GetEventList()
        {
            return _eventRepository.Entities
                .OrderBy(e => e.Name)
                .ToList()
                .MapFrom<Event, DetailsEventModel>();
        }

        public EditSessionModel GetEditSessionModel(Guid speakerId, int sessionId)
        {
            var session = GetSession(speakerId, sessionId);
            return session.MapFrom<Session, EditSessionModel>();
        }

        public void UpdateSession(Guid speakerId, int sessionId, EditSessionModel model)
        {
            var session = GetSession(speakerId, sessionId);
            var theEvent = GetEvent(model.EventId);
            session.Event = theEvent;
            model.MapTo(session);
            _repository.SaveChanges();
        }

        public DisplaySessionModel GetDisplaySessionModel(Guid speakerId, int sessionId)
        {
            return GetSession(speakerId, sessionId).MapFrom<Session, DisplaySessionModel>();
        }

        public void ToogleSelected(Guid speakerId, int sessionId)
        {
            var session = GetSession(speakerId, sessionId);
            session.Selected = !session.Selected;
            _repository.SaveChanges();
        }

        private Session GetSession(Guid speakerId, int sessionId)
        {
            var query = from ss in _repository.Include("Speaker","Event")
                        where ss.Id == sessionId && ss.Speaker.Id == speakerId
                        select ss;
            return query.Single();
        }
    }
}