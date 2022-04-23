
import asyncio
import os

#import UnityEngine

import collections
import enum
import json
import sys
import multidict
import logging
import ssl as ssl_
import struct
from typing import *

import aiohttp
import brotli
#========================== models

class HeartbeatMessage:


    def __init__(
        self,
        popularity: int = None,
    ):
        self.popularity: int = popularity

    @classmethod
    def from_command(cls, data: dict):
        return cls(
            popularity=data['popularity'],
        )


class DanmakuMessage:


    def __init__(
        self,
        mode: int = None,
        font_size: int = None,
        color: int = None,
        timestamp: int = None,
        rnd: int = None,
        uid_crc32: str = None,
        msg_type: int = None,
        bubble: int = None,
        dm_type: int = None,
        emoticon_options: Union[dict, str] = None,
        voice_config: Union[dict, str] = None,
        mode_info: dict = None,

        msg: str = None,

        uid: int = None,
        uname: str = None,
        admin: int = None,
        vip: int = None,
        svip: int = None,
        urank: int = None,
        mobile_verify: int = None,
        uname_color: str = None,

        medal_level: str = None,
        medal_name: str = None,
        runame: str = None,
        medal_room_id: int = None,
        mcolor: int = None,
        special_medal: str = None,

        user_level: int = None,
        ulevel_color: int = None,
        ulevel_rank: str = None,

        old_title: str = None,
        title: str = None,

        privilege_type: int = None,
    ):
        self.mode: int = mode
        self.font_size: int = font_size
        self.color: int = color
        self.timestamp: int = timestamp
        self.rnd: int = rnd
        self.uid_crc32: str = uid_crc32
        self.msg_type: int = msg_type
        self.bubble: int = bubble
        self.dm_type: int = dm_type
        self.emoticon_options: Union[dict, str] = emoticon_options
        self.voice_config: Union[dict, str] = voice_config
        self.mode_info: dict = mode_info

        self.msg: str = msg

        self.uid: int = uid
        self.uname: str = uname
        self.admin: int = admin
        self.vip: int = vip
        self.svip: int = svip
        self.urank: int = urank
        self.mobile_verify: int = mobile_verify
        self.uname_color: str = uname_color

        self.medal_level: str = medal_level
        self.medal_name: str = medal_name
        self.runame: str = runame
        self.medal_room_id: int = medal_room_id
        self.mcolor: int = mcolor
        self.special_medal: str = special_medal

        self.user_level: int = user_level
        self.ulevel_color: int = ulevel_color
        self.ulevel_rank: str = ulevel_rank

        self.old_title: str = old_title
        self.title: str = title

        self.privilege_type: int = privilege_type

    @classmethod
    def from_command(cls, info: dict):
        if len(info[3]) != 0:
            medal_level = info[3][0]
            medal_name = info[3][1]
            runame = info[3][2]
            room_id = info[3][3]
            mcolor = info[3][4]
            special_medal = info[3][5]
        else:
            medal_level = 0
            medal_name = ''
            runame = ''
            room_id = 0
            mcolor = 0
            special_medal = 0

        return cls(
            mode=info[0][1],
            font_size=info[0][2],
            color=info[0][3],
            timestamp=info[0][4],
            rnd=info[0][5],
            uid_crc32=info[0][7],
            msg_type=info[0][9],
            bubble=info[0][10],
            dm_type=info[0][12],
            emoticon_options=info[0][13],
            voice_config=info[0][14],
            mode_info=info[0][15],

            msg=info[1],

            uid=info[2][0],
            uname=info[2][1],
            admin=info[2][2],
            vip=info[2][3],
            svip=info[2][4],
            urank=info[2][5],
            mobile_verify=info[2][6],
            uname_color=info[2][7],

            medal_level=medal_level,
            medal_name=medal_name,
            runame=runame,
            medal_room_id=room_id,
            mcolor=mcolor,
            special_medal=special_medal,

            user_level=info[4][0],
            ulevel_color=info[4][2],
            ulevel_rank=info[4][3],

            old_title=info[5][0],
            title=info[5][1],

            privilege_type=info[7],
        )

    @property
    def emoticon_options_dict(self) -> dict:

        if isinstance(self.emoticon_options, dict):
            return self.emoticon_options
        try:
            return json.loads(self.emoticon_options)
        except (json.JSONDecodeError, TypeError):
            return {}

    @property
    def voice_config_dict(self) -> dict:

        if isinstance(self.voice_config, dict):
            return self.voice_config
        try:
            return json.loads(self.voice_config)
        except (json.JSONDecodeError, TypeError):
            return {}


class GiftMessage:


    def __init__(
        self,
        gift_name: str = None,
        num: int = None,
        uname: str = None,
        face: str = None,
        guard_level: int = None,
        uid: int = None,
        timestamp: int = None,
        gift_id: int = None,
        gift_type: int = None,
        action: str = None,
        price: int = None,
        rnd: str = None,
        coin_type: str = None,
        total_coin: int = None,
        tid: str = None,
    ):
        self.gift_name = gift_name
        self.num = num
        self.uname = uname
        self.face = face
        self.guard_level = guard_level
        self.uid = uid
        self.timestamp = timestamp
        self.gift_id = gift_id
        self.gift_type = gift_type
        self.action = action
        self.price = price
        self.rnd = rnd
        self.coin_type = coin_type
        self.total_coin = total_coin
        self.tid = tid

    @classmethod
    def from_command(cls, data: dict):
        return cls(
            gift_name=data['giftName'],
            num=data['num'],
            uname=data['uname'],
            face=data['face'],
            guard_level=data['guard_level'],
            uid=data['uid'],
            timestamp=data['timestamp'],
            gift_id=data['giftId'],
            gift_type=data['giftType'],
            action=data['action'],
            price=data['price'],
            rnd=data['rnd'],
            coin_type=data['coin_type'],
            total_coin=data['total_coin'],
            tid=data['tid'],
        )


class GuardBuyMessage:


    def __init__(
        self,
        uid: int = None,
        username: str = None,
        guard_level: int = None,
        num: int = None,
        price: int = None,
        gift_id: int = None,
        gift_name: str = None,
        start_time: int = None,
        end_time: int = None,
    ):
        self.uid: int = uid
        self.username: str = username
        self.guard_level: int = guard_level
        self.num: int = num
        self.price: int = price
        self.gift_id: int = gift_id
        self.gift_name: str = gift_name
        self.start_time: int = start_time
        self.end_time: int = end_time

    @classmethod
    def from_command(cls, data: dict):
        return cls(
            uid=data['uid'],
            username=data['username'],
            guard_level=data['guard_level'],
            num=data['num'],
            price=data['price'],
            gift_id=data['gift_id'],
            gift_name=data['gift_name'],
            start_time=data['start_time'],
            end_time=data['end_time'],
        )


class SuperChatMessage:


    def __init__(
        self,
        price: int = None,
        message: str = None,
        message_trans: str = None,
        start_time: int = None,
        end_time: int = None,
        time: int = None,
        id_: int = None,
        gift_id: int = None,
        gift_name: str = None,
        uid: int = None,
        uname: str = None,
        face: str = None,
        guard_level: int = None,
        user_level: int = None,
        background_bottom_color: str = None,
        background_color: str = None,
        background_icon: str = None,
        background_image: str = None,
        background_price_color: str = None,
    ):
        self.price: int = price
        self.message: str = message
        self.message_trans: str = message_trans
        self.start_time: int = start_time
        self.end_time: int = end_time
        self.time: int = time
        self.id: int = id_
        self.gift_id: int = gift_id
        self.gift_name: str = gift_name
        self.uid: int = uid
        self.uname: str = uname
        self.face: str = face
        self.guard_level: int = guard_level
        self.user_level: int = user_level
        self.background_bottom_color: str = background_bottom_color
        self.background_color: str = background_color
        self.background_icon: str = background_icon
        self.background_image: str = background_image
        self.background_price_color: str = background_price_color

    @classmethod
    def from_command(cls, data: dict):
        return cls(
            price=data['price'],
            message=data['message'],
            message_trans=data['message_trans'],
            start_time=data['start_time'],
            end_time=data['end_time'],
            time=data['time'],
            id_=data['id'],
            gift_id=data['gift']['gift_id'],
            gift_name=data['gift']['gift_name'],
            uid=data['uid'],
            uname=data['user_info']['uname'],
            face=data['user_info']['face'],
            guard_level=data['user_info']['guard_level'],
            user_level=data['user_info']['user_level'],
            background_bottom_color=data['background_bottom_color'],
            background_color=data['background_color'],
            background_icon=data['background_icon'],
            background_image=data['background_image'],
            background_price_color=data['background_price_color'],
        )


class SuperChatDeleteMessage:


    def __init__(
        self,
        ids: List[int] = None,
    ):
        self.ids: List[int] = ids

    @classmethod
    def from_command(cls, data: dict):
        return cls(
            ids=data['ids'],
        )


#========================== handlers

logger = logging.getLogger('blivedm')

IGNORED_CMDS = (
    'COMBO_SEND',
    'ENTRY_EFFECT',
    'HOT_RANK_CHANGED',
    'HOT_RANK_CHANGED_V2',
    'INTERACT_WORD',
    'LIVE',
    'LIVE_INTERACTIVE_GAME',
    'NOTICE_MSG',
    'ONLINE_RANK_COUNT',
    'ONLINE_RANK_TOP3',
    'ONLINE_RANK_V2',
    'PK_BATTLE_END',
    'PK_BATTLE_FINAL_PROCESS',
    'PK_BATTLE_PROCESS',
    'PK_BATTLE_PROCESS_NEW',
    'PK_BATTLE_SETTLE',
    'PK_BATTLE_SETTLE_USER',
    'PK_BATTLE_SETTLE_V2',
    'PREPARING',
    'ROOM_REAL_TIME_MESSAGE_UPDATE',
    'STOP_LIVE_ROOM_LIST',
    'SUPER_CHAT_MESSAGE_JPN',
    'WIDGET_BANNER',
)

logged_unknown_cmds = set()





#========================== clients
logger = logging.getLogger('blivedm')

ROOM_INIT_URL = 'https://api.live.bilibili.com/xlive/web-room/v1/index/getInfoByRoom'
DANMAKU_SERVER_CONF_URL = 'https://api.live.bilibili.com/xlive/web-room/v1/index/getDanmuInfo'
DEFAULT_DANMAKU_SERVER_LIST = [
    {'host': 'broadcastlv.chat.bilibili.com', 'port': 2243, 'wss_port': 443, 'ws_port': 2244}
]

HEADER_STRUCT = struct.Struct('>I2H2I')
HeaderTuple = collections.namedtuple('HeaderTuple', ('pack_len', 'raw_header_size', 'ver', 'operation', 'seq_id'))


# WS_BODY_PROTOCOL_VERSION
class ProtoVer(enum.IntEnum):
    NORMAL = 0
    HEARTBEAT = 1
    DEFLATE = 2
    BROTLI = 3


# go-common\app\service\main\broadcast\model\operation.go
class Operation(enum.IntEnum):
    HANDSHAKE = 0
    HANDSHAKE_REPLY = 1
    HEARTBEAT = 2
    HEARTBEAT_REPLY = 3
    SEND_MSG = 4
    SEND_MSG_REPLY = 5
    DISCONNECT_REPLY = 6
    AUTH = 7
    AUTH_REPLY = 8
    RAW = 9
    PROTO_READY = 10
    PROTO_FINISH = 11
    CHANGE_ROOM = 12
    CHANGE_ROOM_REPLY = 13
    REGISTER = 14
    REGISTER_REPLY = 15
    UNREGISTER = 16
    UNREGISTER_REPLY = 17
    # MinBusinessOp = 1000
    # MaxBusinessOp = 10000


# WS_AUTH
class AuthReplyCode(enum.IntEnum):
    OK = 0
    TOKEN_ERROR = -101


class InitError(Exception):
    """AAA"""

class AuthError(Exception):
    """BBB"""

class BLiveClient:

    def __init__(
        self,
        room_id,
        uid=0,
        session: Optional[aiohttp.ClientSession] = None,
        heartbeat_interval=30,
        ssl: Union[bool, ssl_.SSLContext] = True,
        loop: Optional[asyncio.BaseEventLoop] = None,
    ):
        self._tmp_room_id = room_id
        self._uid = uid

        if loop is not None:
            self._loop = loop
        elif session is not None:
            self._loop = session.loop  # noqa
        else:
            self._loop = asyncio.get_event_loop()

        if session is None:
            self._session = aiohttp.ClientSession(loop=self._loop, timeout=aiohttp.ClientTimeout(total=10))
            self._own_session = True
        else:
            self._session = session
            self._own_session = False
            if self._session.loop is not self._loop:  # noqa
                raise RuntimeError('BLiveClient and session must use the same event loop')

        self._heartbeat_interval = heartbeat_interval
        self._ssl = ssl if ssl else ssl_._create_unverified_context()  # noqa

        self._handlers: List[HandlerInterface] = []

        self._room_id = None

        self._room_short_id = None

        self._room_owner_uid = None

        # [{host: "tx-bj4-live-comet-04.chat.bilibili.com", port: 2243, wss_port: 443, ws_port: 2244}, ...]
        self._host_server_list: Optional[List[dict]] = None

        self._host_server_token = None


        self._websocket: Optional[aiohttp.ClientWebSocketResponse] = None

        self._network_future: Optional[asyncio.Future] = None

        self._heartbeat_timer_handle: Optional[asyncio.TimerHandle] = None

    @property
    def is_running(self) -> bool:

        return self._network_future is not None

    @property
    def room_id(self) -> Optional[int]:

        return self._room_id

    @property
    def room_short_id(self) -> Optional[int]:

        return self._room_short_id

    @property
    def room_owner_uid(self) -> Optional[int]:

        return self._room_owner_uid

    def add_handler(self, handler: 'HandlerInterface'):

        if handler not in self._handlers:
            self._handlers.append(handler)

    def remove_handler(self, handler: 'HandlerInterface'):

        try:
            self._handlers.remove(handler)
        except ValueError:
            pass

    def start(self):

        if self.is_running:
            logger.warning('room=%s client is running, cannot start() again', self.room_id)
            return

        self._network_future = asyncio.ensure_future(self._network_coroutine_wrapper(), loop=self._loop)

    def stop(self):

        if not self.is_running:
            logger.warning('room=%s client is stopped, cannot stop() again', self.room_id)
            return

        self._network_future.cancel()

    async def stop_and_close(self):

        if self.is_running:
            self.stop()
            await self.join()
        await self.close()

    async def join(self):

        if not self.is_running:
            logger.warning('room=%s client is stopped, cannot join()', self.room_id)
            return

        await asyncio.shield(self._network_future)

    async def close(self):

        if self.is_running:
            logger.warning('room=%s is calling close(), but client is running', self.room_id)


        if self._own_session:
            await self._session.close()

    async def init_room(self):

        res = True
        if not await self._init_room_id_and_owner():
            res = False

            self._room_id = self._room_short_id = self._tmp_room_id
            self._room_owner_uid = 0

        if not await self._init_host_server():
            res = False

            self._host_server_list = DEFAULT_DANMAKU_SERVER_LIST
            self._host_server_token = None
        return res

    async def _init_room_id_and_owner(self):
        try:
            async with self._session.get(ROOM_INIT_URL, params={'room_id': self._tmp_room_id},
                                         ssl=self._ssl) as res:
                if res.status != 200:
                    logger.warning('room=%d _init_room_id_and_owner() failed, status=%d, reason=%s', self._tmp_room_id,
                                   res.status, res.reason)
                    return False
                data = await res.json()
                if data['code'] != 0:
                    logger.warning('room=%d _init_room_id_and_owner() failed, message=%s', self._tmp_room_id,
                                   data['message'])
                    return False
                if not self._parse_room_init(data['data']):
                    return False
        except (aiohttp.ClientConnectionError, asyncio.TimeoutError):
            logger.exception('room=%d _init_room_id_and_owner() failed:', self._tmp_room_id)
            return False
        return True

    def _parse_room_init(self, data):
        room_info = data['room_info']
        self._room_id = room_info['room_id']
        self._room_short_id = room_info['short_id']
        self._room_owner_uid = room_info['uid']
        return True

    async def _init_host_server(self):
        try:
            async with self._session.get(DANMAKU_SERVER_CONF_URL, params={'id': self._room_id, 'type': 0},
                                         ssl=self._ssl) as res:
                if res.status != 200:
                    logger.warning('room=%d _init_host_server() failed, status=%d, reason=%s', self._room_id,
                                   res.status, res.reason)
                    return False
                data = await res.json()
                if data['code'] != 0:
                    logger.warning('room=%d _init_host_server() failed, message=%s', self._room_id, data['message'])
                    return False
                if not self._parse_danmaku_server_conf(data['data']):
                    return False
        except (aiohttp.ClientConnectionError, asyncio.TimeoutError):
            logger.exception('room=%d _init_host_server() failed:', self._room_id)
            return False
        return True

    def _parse_danmaku_server_conf(self, data):
        self._host_server_list = data['host_list']
        self._host_server_token = data['token']
        if not self._host_server_list:
            logger.warning('room=%d _parse_danmaku_server_conf() failed: host_server_list is empty', self._room_id)
            return False
        return True

    @staticmethod
    def _make_packet(data: dict, operation: int) -> bytes:

        body = json.dumps(data).encode('utf-8')
        header = HEADER_STRUCT.pack(*HeaderTuple(
            pack_len=HEADER_STRUCT.size + len(body),
            raw_header_size=HEADER_STRUCT.size,
            ver=1,
            operation=operation,
            seq_id=1
        ))
        return header + body

    async def _network_coroutine_wrapper(self):

        try:
            await self._network_coroutine()
        except asyncio.CancelledError:

            pass
        except Exception as e:  # noqa
            logger.exception('room=%s _network_coroutine() finished with exception:', self.room_id)
        finally:
            logger.debug('room=%s _network_coroutine() finished', self.room_id)
            self._network_future = None

    async def _network_coroutine(self):

        if self._host_server_token is None:
            if not await self.init_room():
                raise InitError('init_room() failed')

        retry_count = 0
        while True:
            try:

                host_server = self._host_server_list[retry_count % len(self._host_server_list)]
                async with self._session.ws_connect(
                    f"wss://{host_server['host']}:{host_server['wss_port']}/sub",
                    receive_timeout=self._heartbeat_interval + 5,
                    ssl=self._ssl
                ) as websocket:
                    self._websocket = websocket
                    await self._on_ws_connect()


                    message: aiohttp.WSMessage
                    async for message in websocket:
                        await self._on_ws_message(message)

                        retry_count = 0

            except (aiohttp.ClientConnectionError, asyncio.TimeoutError):

                pass
            except AuthError:

                logger.exception('room=%d auth failed, trying init_room() again', self.room_id)
                if not await self.init_room():
                    raise InitError('init_room() failed')
            except ssl_.SSLError:
                logger.error('room=%d a SSLError happened, cannot reconnect', self.room_id)
                raise
            finally:
                self._websocket = None
                await self._on_ws_close()

            retry_count += 1
            logger.warning('room=%d is reconnecting, retry_count=%d', self.room_id, retry_count)
            await asyncio.sleep(1, loop=self._loop)

    async def _on_ws_connect(self):

        await self._send_auth()
        self._heartbeat_timer_handle = self._loop.call_later(self._heartbeat_interval, self._on_send_heartbeat)

    async def _on_ws_close(self):

        if self._heartbeat_timer_handle is not None:
            self._heartbeat_timer_handle.cancel()
            self._heartbeat_timer_handle = None

    async def _send_auth(self):

        auth_params = {
            'uid': self._uid,
            'roomid': self._room_id,
            'protover': 3,
            'platform': 'web',
            'type': 2
        }
        if self._host_server_token is not None:
            auth_params['key'] = self._host_server_token
        await self._websocket.send_bytes(self._make_packet(auth_params, Operation.AUTH))

    def _on_send_heartbeat(self):

        if self._websocket is None or self._websocket.closed:
            self._heartbeat_timer_handle = None
            return

        self._heartbeat_timer_handle = self._loop.call_later(self._heartbeat_interval, self._on_send_heartbeat)
        asyncio.ensure_future(self._send_heartbeat(), loop=self._loop)

    async def _send_heartbeat(self):

        if self._websocket is None or self._websocket.closed:
            return

        try:
            await self._websocket.send_bytes(self._make_packet({}, Operation.HEARTBEAT))
        except (ConnectionResetError, aiohttp.ClientConnectionError) as e:
            logger.warning('room=%d _send_heartbeat() failed: %r', self.room_id, e)
        except Exception:  # noqa
            logger.exception('room=%d _send_heartbeat() failed:', self.room_id)

    async def _on_ws_message(self, message: aiohttp.WSMessage):

        if message.type != aiohttp.WSMsgType.BINARY:
            logger.warning('room=%d unknown websocket message type=%s, data=%s', self.room_id,
                           message.type, message.data)
            return

        try:
            await self._parse_ws_message(message.data)
        except (asyncio.CancelledError, AuthError):

            raise
        except Exception:  # noqa
            logger.exception('room=%d _parse_ws_message() error:', self.room_id)

    async def _parse_ws_message(self, data: bytes):

        offset = 0
        try:
            header = HeaderTuple(*HEADER_STRUCT.unpack_from(data, offset))
        except struct.error:
            logger.exception('room=%d parsing header failed, offset=%d, data=%s', self.room_id, offset, data)
            return

        if header.operation in (Operation.SEND_MSG_REPLY, Operation.AUTH_REPLY):

            while True:
                body = data[offset + header.raw_header_size: offset + header.pack_len]
                await self._parse_business_message(header, body)

                offset += header.pack_len
                if offset >= len(data):
                    break

                try:
                    header = HeaderTuple(*HEADER_STRUCT.unpack_from(data, offset))
                except struct.error:
                    logger.exception('room=%d parsing header failed, offset=%d, data=%s', self.room_id, offset, data)
                    break

        elif header.operation == Operation.HEARTBEAT_REPLY:

            body = data[offset + header.raw_header_size: offset + header.raw_header_size + 4]
            popularity = int.from_bytes(body, 'big')

            body = {
                'cmd': '_HEARTBEAT',
                'data': {
                    'popularity': popularity
                }
            }
            await self._handle_command(body)

        else:

            body = data[offset + header.raw_header_size: offset + header.pack_len]
            logger.warning('room=%d unknown message operation=%d, header=%s, body=%s', self.room_id,
                           header.operation, header, body)

    async def _parse_business_message(self, header: HeaderTuple, body: bytes):

        if header.operation == Operation.SEND_MSG_REPLY:

            if header.ver == ProtoVer.BROTLI:

                body = await self._loop.run_in_executor(None, brotli.decompress, body)
                await self._parse_ws_message(body)
            elif header.ver == ProtoVer.NORMAL:

                if len(body) != 0:
                    try:
                        body = json.loads(body.decode('utf-8'))
                        await self._handle_command(body)
                    except asyncio.CancelledError:
                        raise
                    except Exception:
                        logger.error('room=%d, body=%s', self.room_id, body)
                        raise
            else:

                logger.warning('room=%d unknown protocol version=%d, header=%s, body=%s', self.room_id,
                               header.ver, header, body)

        elif header.operation == Operation.AUTH_REPLY:

            body = json.loads(body.decode('utf-8'))
            if body['code'] != AuthReplyCode.OK:
                raise AuthError(f"auth reply error, code={body['code']}, body={body}")
            await self._websocket.send_bytes(self._make_packet({}, Operation.HEARTBEAT))

        else:

            logger.warning('room=%d unknown message operation=%d, header=%s, body=%s', self.room_id,
                           header.operation, header, body)

    async def _handle_command(self, command: dict):


        results = await asyncio.shield(
            asyncio.gather(
                *(handler.handle(self, command) for handler in self._handlers),
                loop=self._loop,
                return_exceptions=True
            ),
            loop=self._loop
        )
        for res in results:
            if isinstance(res, Exception):
                logger.exception('room=%d _handle_command() failed, command=%s', self.room_id, command, exc_info=res)

class HandlerInterface:

    async def handle(self, client: BLiveClient, command: dict):
        raise NotImplementedError


class BaseHandler(HandlerInterface):

    def __heartbeat_callback(self, client: BLiveClient, command: dict):
        return self._on_heartbeat(client, HeartbeatMessage.from_command(command['data']))

    def __danmu_msg_callback(self, client: BLiveClient, command: dict):
        return self._on_danmaku(client, DanmakuMessage.from_command(command['info']))

    def __send_gift_callback(self, client: BLiveClient, command: dict):
        return self._on_gift(client, GiftMessage.from_command(command['data']))

    def __guard_buy_callback(self, client: BLiveClient, command: dict):
        return self._on_buy_guard(client, GuardBuyMessage.from_command(command['data']))

    def __super_chat_message_callback(self, client: BLiveClient, command: dict):
        return self._on_super_chat(client, SuperChatMessage.from_command(command['data']))

    def __super_chat_message_delete_callback(self, client: BLiveClient, command: dict):
        return self._on_super_chat_delete(client, SuperChatDeleteMessage.from_command(command['data']))

    _CMD_CALLBACK_DICT: Dict[
        str,
        Optional[Callable[
            ['BaseHandler', BLiveClient, dict],
            Awaitable
        ]]
    ] = {
        '_HEARTBEAT': __heartbeat_callback,

        'DANMU_MSG': __danmu_msg_callback,

        'SEND_GIFT': __send_gift_callback,

        'GUARD_BUY': __guard_buy_callback,

        'SUPER_CHAT_MESSAGE': __super_chat_message_callback,

        'SUPER_CHAT_MESSAGE_DELETE': __super_chat_message_delete_callback,
    }

    for cmd in IGNORED_CMDS:
        _CMD_CALLBACK_DICT[cmd] = None
    del cmd

    async def handle(self, client: BLiveClient, command: dict):
        cmd = command.get('cmd', '')
        pos = cmd.find(':')  
        if pos != -1:
            cmd = cmd[:pos]

        if cmd not in self._CMD_CALLBACK_DICT:

            if cmd not in logged_unknown_cmds:
                logger.warning('room=%d unknown cmd=%s, command=%s', client.room_id, cmd, command)
                logged_unknown_cmds.add(cmd)
            return

        callback = self._CMD_CALLBACK_DICT[cmd]
        if callback is not None:
            await callback(self, client, command)

    async def _on_heartbeat(self, client: BLiveClient, message: HeartbeatMessage):
        """
        AAA
        """

    async def _on_danmaku(self, client: BLiveClient, message: DanmakuMessage):
        """
        AAA
        """

    async def _on_gift(self, client: BLiveClient, message: GiftMessage):
        """
        AAA
        """

    async def _on_buy_guard(self, client: BLiveClient, message: GuardBuyMessage):
        """
        AAA
        """

    async def _on_super_chat(self, client: BLiveClient, message: SuperChatMessage):
        """
        AAA
        """

    async def _on_super_chat_delete(self, client: BLiveClient, message: SuperChatDeleteMessage):
        """
        AAA
        """

#========================== Origin
async def main():
    await run_single_client()


async def run_single_client():

    room_id = sys.argv[1]

    client = BLiveClient(room_id, ssl=True)
    handler = MyHandler()
    client.add_handler(handler)

    client.start()
    try:

        await asyncio.sleep(50000000)
        client.stop()

        await client.join()
    finally:
        await client.stop_and_close()

class MyHandler(BaseHandler):

    sys.stdout.reconfigure(encoding='utf-8')

    async def _on_heartbeat(self, client: BLiveClient, message: HeartbeatMessage):
        # sys.stdout.write('\r' + f'R[{client.room_id}] 当前人气: {message.popularity}')
        sys.stdout.write('\r' + f'R{client.room_id}$#**#${message.popularity}')
        sys.stdout.flush()

        
    async def _on_danmaku(self, client: BLiveClient, message: DanmakuMessage):
        # sys.stdout.write('\r' + f'D[{client.room_id}] {message.uname}: {message.msg}')
        sys.stdout.write('\r' + f'D{client.room_id}$#**#${message.uname}$#**#${message.msg}')
        sys.stdout.flush()
        sys.stdout.write('\r ')
        sys.stdout.flush()
        

    async def _on_gift(self, client: BLiveClient, message: GiftMessage):
        # sys.stdout.write('\r' + f'G[{client.room_id}] {message.uname} 赠送了 {message.gift_name}x{message.num}'
        #       f' ({message.coin_type} 瓜子 x {message.total_coin})')
        sys.stdout.write('\r' + f'G{client.room_id}$#**#${message.uname}$#**#${message.gift_name}$#**#${message.num}'
              f'$#**#${message.coin_type}$#**#${message.total_coin}')
        
        sys.stdout.flush()
        sys.stdout.write('\r ')
        sys.stdout.flush()


    async def _on_buy_guard(self, client: BLiveClient, message: GuardBuyMessage):
        # sys.stdout.write('\r' + f'J[{client.room_id}] {message.username} 购买了 {message.gift_name}')
        sys.stdout.write('\r' + f'J{client.room_id}$#**#${message.username}$#**#${message.gift_name}')
        sys.stdout.flush()
        sys.stdout.write('\r ')
        sys.stdout.flush()


    async def _on_super_chat(self, client: BLiveClient, message: SuperChatMessage):
        # sys.stdout.write('\r' + f'S[{client.room_id}] 醒目留言 ￥{message.price} {message.uname}：{message.message}')
        sys.stdout.write('\r' + f'S{client.room_id}$#**#${message.price}$#**#${message.uname}$#**#${message.message}')
        sys.stdout.flush()
        sys.stdout.write('\r ')
        sys.stdout.flush()


asyncio.get_event_loop().run_until_complete(main())
