import { Component, OnInit, ViewChild } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment'
import { stringify } from '@angular/compiler/src/util';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent implements OnInit {
  @ViewChild('loadingMask') loadingMask: any;
  public server: string;
  public vpnName: string;
  constructor(private client: HttpClient) { }

  ngOnInit(): void {
    this.server = environment.server;
    this.vpnName = environment.vpnName;
  }
  ngAfterViewInit() {
    this.loadingMask.nativeElement.style.display = 'none';
  }
  async connectToVPN() {
    if (!this.server.trim() || !this.vpnName.trim()) {
      alert("请输入服务器和VPN!");
      return;
    }
    this.loadingMask.nativeElement.style.display = 'flex';
    try {
      let result: any = await this.client.get(`http://${this.server}/javbookwebapi/csc/connect?vpn=${this.vpnName}`).toPromise();
      this.loadingMask.nativeElement.style.display = 'none';
      if (result) alert("连接成功");
      else alert("连接失败");
    } catch (error) {
      this.loadingMask.nativeElement.style.display = 'none';
      alert("连接失败");
    }
  }
  async disconnectToVPN() {
    if (!this.server.trim()) {
      alert("请输入服务器!");
      return;
    }
    this.loadingMask.nativeElement.style.display = 'flex';
    try {
      let result: any = await this.client.get(`http://${this.server}/javbookwebapi/csc/disconnect`).toPromise();
      this.loadingMask.nativeElement.style.display = 'none';
      if (result) alert("断开连接成功");
      else alert("断开连接失败");
    } catch (error) {
      this.loadingMask.nativeElement.style.display = 'none';
      alert("断开连接失败");
    }
  }
}
